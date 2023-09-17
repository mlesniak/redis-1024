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
        // Commands are send as serialized arrays.
        var commands = RedisType.Deserialize<RedisArray>(stream);
        return Execute(commands);
    }

    byte[] Execute(RedisArray commandline)
    {
        List<RedisType> parts = commandline.Values!;
        var command = ((RedisString)parts[0]).Value!.ToLower();
        var arguments = parts.Skip(1).ToList();

        RedisType result = command switch
        {
            "set" => SetHandler(arguments),
            "get" => GetHandler(arguments),
            _ => UnknownCommandHandler()
        };

        return result.Serialize();
    }

    private RedisType SetHandler(IReadOnlyList<RedisType> arguments)
    {
        var setKey = ((RedisString)arguments[0]).Value!;
        byte[] value = Encoding.ASCII.GetBytes(((RedisString)arguments[1]).Value!);

        int? expirationInMs = null;
        if (arguments.Count > 2)
        {
            var type = ((RedisString)arguments[2]).Value!;
            var num = Int32.Parse(((RedisString)arguments[3]).Value!);

            expirationInMs = type.ToLower() switch
            {
                "ex" => num * 1_000,
                "px" => num,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        _database.Set(setKey, value, expirationInMs);
        return RedisString.From("OK");
    }

    private RedisType GetHandler(IReadOnlyList<RedisType> arguments)
    {
        var getKey = ((RedisString)arguments[0]).Value!;
        var resultBytes = _database.Get(getKey);
        return resultBytes == null
            ? RedisString.Nil()
            : RedisString.From(resultBytes);
    }

    private RedisType UnknownCommandHandler()
    {
        return RedisString.From("-UNKNOWN COMMAND");
    }
}