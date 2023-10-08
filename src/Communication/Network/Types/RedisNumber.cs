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

    protected bool Equals(RedisNumber other) => Value == other.Value;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj.GetType() != this.GetType())
        {
            return false;
        }
        return Equals((RedisNumber)obj);
    }

    public override int GetHashCode() => Value.GetHashCode();
}
