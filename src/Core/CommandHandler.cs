using System.Text;

using Lesniak.Redis.Core.Model;
using Lesniak.Redis.Storage;

namespace Lesniak.Redis.Core;

public class CommandHandler
{
    private readonly Memory _memory;

    public CommandHandler(Memory memory)
    {
        _memory = memory;
    }
    
    public byte[] Execute(RedisArray commandline)
    {
        List<RedisType> arrayValues = commandline.Values!;
        var command = ((RedisString)arrayValues[0]).Value!;
        switch (command)
        {
            // TODO(mlesniak) expiration time
            case "set":
                var setKey = ((RedisString)arrayValues[1]).Value!;
                byte[] value = Encoding.ASCII.GetBytes(((RedisString)arrayValues[2]).Value!);
                _memory.Set(setKey, value);
                return "+OK\r\n"u8.ToArray();
            case "get":
                var getKey = ((RedisString)arrayValues[1]).Value!;
                var resultBytes = _memory.Get(getKey);
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