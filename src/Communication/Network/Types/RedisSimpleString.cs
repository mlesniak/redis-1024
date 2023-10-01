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
}