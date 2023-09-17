using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

public class RedisInt : RedisType
{
    public const char Identifier = ':';

    private RedisInt(long value)
    {
        Value = value;
    }

    public long Value { get; }

    public static new (RedisType, int) Deserialize(byte[] data, int offset)
    {
        var lengthEnd = Array.IndexOf(data, (byte)'\r', offset);
        var length = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, lengthEnd - offset - 1));
        int stringStart = lengthEnd + 2;
        int value = Int32.Parse(Encoding.ASCII.GetString(data, stringStart, length));
        RedisInt result = new(value);
        return (result, stringStart + length + 2);
    }

    public override byte[] Serialize()
    {
        var sb = new StringBuilder();
        sb.Append($":{Value}");
        sb.Append("\r\n");
        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    public static RedisInt From(long l) => new(l);
}