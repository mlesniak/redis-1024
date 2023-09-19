using System.Text;

using Lesniak.Redis.Communication.Network.Types;
using Lesniak.Redis.Core;

namespace Lesniak.Redis.Communication.Network;

public class CommandHandler
{
    private readonly IDatabase _database;

    public CommandHandler(IDatabase database)
    {
        _database = database;
    }

    public byte[] Execute(byte[] stream)
    {
        int offset = 0;
        List<byte> responses = new();
        while (offset < stream.Length)
        {
            // Commands are send as serialized arrays.
            var (commands, nextOffset) = RedisType.Deserialize<RedisArray>(stream, offset);
            var singleResponse = Execute(commands);
            responses.AddRange(singleResponse);
            offset = nextOffset;
        }

        return responses.ToArray();
    }

    byte[] Execute(RedisArray commandline)
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
            _ => UnknownCommandHandler()
        };

        return result.Serialize();
    }

    private RedisType SubscribeHandler(List<RedisType> arguments)
    {
        // Hack to make the real client work.
        // We currently do not support channels,
        // but it's on our roadmap.
        return RedisArray.From(
            RedisBulkString.From("subscribe"),
            RedisBulkString.From(((RedisBulkString)arguments[0]).Value!),
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
}