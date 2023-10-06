using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

public class RedisBulkString : RedisValue
{
    public const char Identifier = '$';

    private RedisBulkString(byte[]? value)
    {
        Value = value;
    }

    public byte[]? Value { get; }

    public string AsciiValue
    {
        get => Encoding.ASCII.GetString(Value!);
    }

    public static (RedisValue, int) Deserialize(byte[] data, int offset)
    {
        int lengthEnd = Array.IndexOf(data, (byte)'\r', offset);
        int length = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, lengthEnd - offset - 1));
        int stringStart = lengthEnd + 2;
        RedisBulkString result = new(data.AsSpan(stringStart, length).ToArray());
        return (result, stringStart + length + 2);
    }

    public override byte[] Serialize()
    {
        if (Value == null)
        {
            return "$-1\r\n"u8.ToArray();
        }

        List<byte> bytes = new();
        byte[] prefixBytes = Encoding.ASCII.GetBytes($"${Value!.Length}\r\n");
        byte[] suffix = "\r\n"u8.ToArray();
        bytes.AddRange(prefixBytes);
        bytes.AddRange(Value!);
        bytes.AddRange(suffix);
        return bytes.ToArray();
    }

    public static RedisBulkString From(string? s)
    {
        return new RedisBulkString(Encoding.ASCII.GetBytes(s));
    }

    public static RedisBulkString From(byte[] bs)
    {
        return new RedisBulkString(bs);
    }

    public static RedisBulkString Nil()
    {
        return new RedisBulkString(null);
    }
}
