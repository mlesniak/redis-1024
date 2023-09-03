using System.Text;

namespace Lesniak.Redis.Core.Model;

public class RedisString : RedisType
{
    public const char Identifier = '$';

    private RedisString(string? value)
    {
        Value = value;
    }

    public string? Value { get; }

    public static new (RedisType, int) Deserialize(byte[] data, int offset)
    {
        var lengthEnd = Array.IndexOf(data, (byte)'\r', offset);
        var length = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, lengthEnd - offset - 1));
        int stringStart = lengthEnd + 2;
        RedisString result = new(Encoding.ASCII.GetString(data, stringStart, length));
        return (result, stringStart + length + 2);
    }
    
    public static RedisString From(string? s) => new(s);

    public static RedisString From(byte[] bs) => new(Encoding.ASCII.GetString(bs));

    public static RedisString Nil() => new(null);

    public override byte[] Serialize()
    {
        var sb = new StringBuilder();

        if (Value == null)
        {
            sb.Append("$-1");
        }
        else
        {
            sb.Append($"${Value!.Length}");
            sb.Append("\r\n");
            sb.Append(Value);
        }

        sb.Append("\r\n");
        return Encoding.ASCII.GetBytes(sb.ToString());
    }
}