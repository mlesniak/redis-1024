using System.Net.Sockets;
using System.Text;

using Lesniak.Redis.Communication.Network.Types;
using Lesniak.Redis.Core;
using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Communication.Network;

public class ClientHandler
{
    private static readonly ILogger log = Logging.For<ClientHandler>();
    private readonly IDatabase _database;
    private NetworkStream _stream;
    private string _clientId;

    public ClientHandler(string clientId, IDatabase database, NetworkStream stream)
    {
        _clientId = clientId;
        _stream = stream;
        _database = database;
    }

    public byte[] Handle(byte[] stream)
    {
        int offset = 0;
        List<byte> responses = new();
        while (offset < stream.Length)
        {
            // Commands are send as serialized arrays.
            var (commands, nextOffset) = RedisType.Deserialize<RedisArray>(stream, offset);
            var singleResponse = ExecuteCommand(commands);
            responses.AddRange(singleResponse);
            offset = nextOffset;
        }

        return responses.ToArray();
    }

    // TODO(mlesniak) Move to something like CommandParser?
    byte[] ExecuteCommand(RedisArray commandline)
    {
        IList<RedisType> parts = commandline.Values!;
        var rs = ((RedisBulkString)parts[0]).Value!;
        var command = Encoding.ASCII.GetString(rs).ToLower();
        var arguments = parts.Skip(1).ToList();

        RedisType result = command switch
        {
            "set" => SetHandler(arguments),
            "get" => GetHandler(arguments),
            "echo" => EchoHandler(arguments),
            "subscribe" => SubscribeHandler(arguments),
            "publish" => PublishHandler(arguments),
            _ => UnknownCommandHandler()
        };

        return result.Serialize();
    }

    private RedisType PublishHandler(List<RedisType> arguments)
    {
        var channel = ((RedisBulkString)arguments[0]).ToAsciiString();
        var message = ((RedisBulkString)arguments[1]).Value!;

        _database.Publish(channel, message);
        return RedisNumber.From(1);
    }

    private RedisType SubscribeHandler(List<RedisType> arguments)
    {
        var channel = ((RedisBulkString)arguments[0]).ToAsciiString();

        _database.Subscribe(channel, (c, message) =>
        {
            log.LogInformation("Received message on channel {Channel}: {S}", c, Encoding.UTF8.GetString(message));
            var response = RedisArray.From(
                RedisBulkString.From("message"),
                RedisBulkString.From(c),
                RedisBulkString.From(message)
            ).Serialize();
            _stream.Write(response, 0, response.Length);
        });

        // TODO(mlesniak) add subscription number.
        return RedisArray.From(
            RedisBulkString.From("subscribe"),
            RedisBulkString.From(((RedisBulkString)arguments[0]).Value!),
            // TODO(mlesniak) should contain the actual number
            //                of subscribes for this channel
            RedisNumber.From(1)
        );
    }

    private RedisType EchoHandler(List<RedisType> arguments)
    {
        var response = ((RedisBulkString)arguments[0]).Value!;
        return RedisBulkString.From(response);
    }

    private RedisType SetHandler(IReadOnlyList<RedisType> arguments)
    {
        var setKey = ((RedisBulkString)arguments[0]).ToAsciiString();
        byte[] value = ((RedisBulkString)arguments[1]).Value!;

        int? expirationInMs = null;
        if (arguments.Count > 2)
        {
            var type = ((RedisBulkString)arguments[2]).ToAsciiString();
            var num = Int32.Parse(((RedisBulkString)arguments[3]).ToAsciiString());

            expirationInMs = type.ToLower() switch
            {
                "ex" => num * 1_000,
                "px" => num,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        _database.Set(setKey, value, expirationInMs);
        return RedisSimpleString.From("OK");
    }

    private RedisType GetHandler(IReadOnlyList<RedisType> arguments)
    {
        var getKey = ((RedisBulkString)arguments[0]).ToAsciiString();
        var resultBytes = _database.Get(getKey);
        return resultBytes == null
            ? RedisBulkString.Nil()
            : RedisBulkString.From(resultBytes);
    }

    private RedisType UnknownCommandHandler()
    {
        return RedisErrorString.From("ERR UNKNOWN COMMAND");
    }

    public Task Run()
    {
        log.LogInformation("Client {Id} connected", _clientId);
        while (await NetworkUtils.ReadAsync(_stream, Configuration.Get().MaxReadBuffer) is { } readBytes)
        {
            var responseBytes = Handle(readBytes);
            await _stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }

        log.LogInformation("Client {Id} disconnected", _clientId);
    }
}