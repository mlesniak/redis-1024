using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

public class RedisNumber : RedisValue
{
    public const char Identifier = ':';

    private RedisNumber(long value)
    {
        Value = value;
    }

    public long Value { get; }

    public static (RedisValue, int) Deserialize(byte[] data, int offset)
    {
        int lengthEnd = Array.IndexOf(data, (byte)'\r', offset);
        int length = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, lengthEnd - offset - 1));
        int stringStart = lengthEnd + 2;
        int value = Int32.Parse(Encoding.ASCII.GetString(data, stringStart, length));
        RedisNumber result = new(value);
        return (result, stringStart + length + 2);
    }

    public override byte[] Serialize()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($":{Value}");
        sb.Append("\r\n");
        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    public static RedisNumber From(long l)
    {
        return new RedisNumber(l);
    }
}