using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

public record RedisErrorString : RedisValue
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

    public override int GetHashCode() => _value.GetHashCode();
}
