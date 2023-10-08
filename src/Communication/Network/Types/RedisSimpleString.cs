using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

public class RedisSimpleString : RedisValue
{
    private const char Identifier = '+';
    private readonly string _value;

    private RedisSimpleString(string value)
    {
        _value = value;
    }

    public override byte[] Serialize()
    {
        string result = $"{Identifier}{_value}\r\n";
        return Encoding.ASCII.GetBytes(result);
    }

    public static RedisSimpleString From(string? s)
    {
        return new RedisSimpleString(s);
    }

    // TODO(mlesniak) Could we create a record instead?
    protected bool Equals(RedisSimpleString other) => _value == other._value;

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
        return Equals((RedisSimpleString)obj);
    }

    public override int GetHashCode() => _value.GetHashCode();
}
