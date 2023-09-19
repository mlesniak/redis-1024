using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

public class RedisErrorString : RedisType
{
    const char Identifier = '-';
    private readonly string _value;

    private RedisErrorString(string value)
    {
        _value = value;
    }

    public override byte[] Serialize()
    {
        var result = $"{Identifier}{_value}\r\n";
        return Encoding.ASCII.GetBytes(result);
    }

    public static RedisErrorString From(string? s) => new(s);
}