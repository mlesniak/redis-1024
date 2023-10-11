using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

public record RedisNumber(long Value) : RedisValue
{
    public const char Identifier = ':';

    public static (RedisValue, int) Deserialize(byte[] data, int offset)
    {
        int lengthEnd = Array.IndexOf(data, (byte)'\r', offset);
        long value = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, lengthEnd - offset - 1));
        RedisNumber result = new(value);
        return (result, lengthEnd + 2);
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

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{Value}";
}
