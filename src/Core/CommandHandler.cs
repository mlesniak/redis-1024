using System.Text;

namespace Lesniak.Redis;

public class CommandHandler
{
    private readonly Memory _memory;

    public CommandHandler(Memory memory)
    {
        _memory = memory;
    }
    
    public byte[] Execute(RedisData commandline)
    {
        List<RedisData> arrayValues = commandline.ArrayValues!;
        var command = arrayValues[0].BulkString!;
        switch (command)
        {
            case "set":
                var setKey = arrayValues[1].BulkString!;
                byte[] value = arrayValues[2].Type switch
                {
                    RedisDataType.BulkString => Encoding.ASCII.GetBytes(arrayValues[2].BulkString!),
                    _ => throw new ArgumentOutOfRangeException()
                };
                _memory.Set(setKey, value);
                return "+OK\r\n"u8.ToArray();
            case "get":
                var getKey = arrayValues[1].BulkString!;
                var resultBytes = _memory.Get(getKey);
                if (resultBytes == null)
                {
                    return RedisData.nil().ToRedisSerialization();
                }

                // Create BulkString as response for now.
                return RedisData.of(resultBytes!).ToRedisSerialization();
            default:
                // Ignoring it for now.
                return "-UNKNOWN COMMAND\r\n"u8.ToArray();
        }
    }
}