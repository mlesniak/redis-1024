using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

public record RedisSimpleString(string Value) : RedisValue
{
    private const char Identifier = '+';

    public override byte[] Serialize()
    {
        string result = $"{Identifier}{Value}\r\n";
        return Encoding.ASCII.GetBytes(result);
    }

    public static RedisSimpleString From(string? s)
    {
        return new RedisSimpleString(s);
    }
}
