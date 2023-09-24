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

    public ClientHandler(IDatabase database)
    {
        _database = database;
    }

    public byte[] Handle(ClientContext ctx, byte[] stream)
    {
        int offset = 0;
        List<byte> responses = new();
        while (offset < stream.Length)
        {
            // Commands are send as serialized arrays.
            var (commands, nextOffset) = RedisType.Deserialize<RedisArray>(stream, offset);
            var singleResponse = ExecuteCommand(ctx, commands);
            responses.AddRange(singleResponse);
            offset = nextOffset;
        }

        return responses.ToArray();
    }

    byte[] ExecuteCommand(ClientContext ctx, RedisArray commandline)
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
            "subscribe" => SubscribeHandler(ctx, arguments),
            "publish" => PublishHandler(arguments),
            _ => UnknownCommandHandler()
        };

        return result.Serialize();
    }

    private RedisType PublishHandler(List<RedisType> arguments)
    {
        var channel = ((RedisBulkString)arguments[0]).ToAsciiString();
        var message = ((RedisBulkString)arguments[1]).Value!;

        var sendTo = _database.Publish(channel, message);
        return RedisNumber.From(sendTo);
    }

    // TODO(mlesniak) support multiple channel per single subscription
    private RedisType SubscribeHandler(ClientContext ctx, List<RedisType> arguments)
    {
        var channel = ((RedisBulkString)arguments[0]).ToAsciiString();
        _database.Subscribe(channel, ResponseAction);
        ctx.NumSubscriptions++;
        return RedisArray.From(
            RedisBulkString.From("subscribe"),
            RedisBulkString.From(((RedisBulkString)arguments[0]).Value!),
            RedisNumber.From(ctx.NumSubscriptions)
        );

        void ResponseAction(string c, byte[] message)
        {
            log.LogTrace("Received message on channel {Channel}: {S}", c, Encoding.UTF8.GetString(message));
            var response = RedisArray.From(
                    RedisBulkString.From("message"),
                    RedisBulkString.From(c),
                    RedisBulkString.From(message))
                .Serialize();
            ctx.SendToClient(response);
        }
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
}