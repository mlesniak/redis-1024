using System.Text;

using Lesniak.Redis.Core.Model;
using Lesniak.Redis.Core.Storage;

namespace Lesniak.Redis.Core;

public class CommandHandler
{
    private readonly InMemory _inMemory;

    public CommandHandler(InMemory inMemory)
    {
        _inMemory = inMemory;
    }
    
    public byte[] Execute(RedisArray commandline)
    {
        List<RedisType> array = commandline.Values!;
        var command = ((RedisString)array[0]).Value!;
        switch (command)
        {
            case "set":
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
                
                _inMemory.Set(setKey, value, expirationInMs);
                return "+OK\r\n"u8.ToArray();
            case "get":
                var getKey = ((RedisString)array[1]).Value!;
                var resultBytes = _inMemory.Get(getKey);
                if (resultBytes == null)
                {
                    return RedisString.Nil().Serialize();
                }

                // Create BulkString as response for now.
                return RedisString.From(resultBytes).Serialize();
            default:
                // Ignoring it for now.
                return "-UNKNOWN COMMAND\r\n"u8.ToArray();
        }
    }
}