using System.Text;

using Lesniak.Redis.Communication.Network.Types;
using Lesniak.Redis.Core;
using Lesniak.Redis.Core.Model;

namespace Lesniak.Redis.Communication.Network;

public class CommandHandler
{
    private readonly IDatabase _database;

    public CommandHandler(IDatabase database)
    {
        _database = database;
    }

    // Commands are send as serialized arrays, which is
    // the main reason we handle them as such internally
    // as well.
    public byte[] Execute(RedisArray commandline)
    {
        List<RedisType> commandParts = commandline.Values!;
        var command = ((RedisString)commandParts[0]).Value!.ToLower();
        return command switch
        {
            "set" => SetHandler(commandParts),
            "get" => GetHandler(commandParts),
            _ => UnknownCommandHandler()
        };
    }

    private byte[] SetHandler(IReadOnlyList<RedisType> array)
    {
        var setKey = ((RedisString)array[1]).Value!;
        byte[] value = Encoding.ASCII.GetBytes(((RedisString)array[2]).Value!);

        int? expirationInMs = null;
        if (array.Count > 3)
        {
            var type = ((RedisString)array[3]).Value!;
            var num = Int32.Parse(((RedisString)array[4]).Value!);

            expirationInMs = type.ToLower() switch
            {
                "ex" => num * 1_000,
                "px" => num,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        _database.Set(setKey, value, expirationInMs);
        return "+OK\r\n"u8.ToArray();
    }

    private byte[] GetHandler(IReadOnlyList<RedisType> commandParts)
    {
        var getKey = ((RedisString)commandParts[1]).Value!;
        var resultBytes = _database.Get(getKey);
        return resultBytes == null
            ? RedisString.Nil().Serialize()
            : RedisString.From(resultBytes).Serialize();
    }

    private byte[] UnknownCommandHandler()
    {
        return "-UNKNOWN COMMAND\r\n"u8.ToArray();
    }
}