using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

public class RedisErrorString : RedisValue
{
    private const char Identifier = '-';
    private readonly string _value;

    private RedisErrorString(string value)
    {
        _value = value;
    }

    public override byte[] Serialize()
    {
        string result = $"{Identifier}{_value}\r\n";
        return Encoding.ASCII.GetBytes(result);
    }

    public static RedisErrorString From(string? s)
    {
        return new RedisErrorString(s);
    }

    protected bool Equals(RedisErrorString other) => _value == other._value;

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
        return Equals((RedisErrorString)obj);
    }

    public override int GetHashCode() => _value.GetHashCode();
}
