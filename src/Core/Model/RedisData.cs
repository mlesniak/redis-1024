using System.Text;

using Microsoft.Extensions.Primitives;

namespace Lesniak.Redis.Core.Model;

public abstract class RedisType
{
    public abstract byte[] Serialize();

    public static RedisType Deserialize(byte[] data) => Deserialize(data, 0).Item1;

    private delegate (RedisType, int) Parser(byte[] data, int offset);

    private static (RedisType, int) ParseArray(byte[] data, int offset)
    {
        RedisArray result = new();
        var numElementsIndexEnd = Array.IndexOf(data, (byte)'\r', offset);
        var numElements =
            Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, numElementsIndexEnd - offset - 1));
        offset = numElementsIndexEnd + 2;
        for (var i = 0; i < numElements; i++)
        {
            (RedisType elem, int nextArrayOffset) = Deserialize(data, offset);
            result.Values.Add(elem);
            offset = nextArrayOffset;
        }

        return (result, offset);
    }

    private static (RedisType, int) ParseBulkString(byte[] data, int offset)
    {
        var lengthEnd = Array.IndexOf(data, (byte)'\r', offset);
        var length = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, lengthEnd - offset - 1));
        int stringStart = lengthEnd + 2;
        RedisString result = new(Encoding.ASCII.GetString(data, stringStart, length));
        return (result, stringStart + length + 2);
    }

    private static (RedisType, int) Deserialize(byte[] data, int offset)
    {
        Parser method = (char)data[offset] switch
        {
            '$' => ParseBulkString,
            '*' => ParseArray,
            _ => throw new ArgumentOutOfRangeException()
        };
        return method(data, offset);
    }

    // TODO(mlesniak)  What if I use a Span here? Then, no offset would be
    //                 needed for the deserialize part?

    // TODO(mlesniak) Strange design decision?
    public override string ToString() =>
        Encoding.ASCII.GetString(Serialize());

    public static RedisString Of(string? s) => new RedisString(s);

    // TODO(mlesniak) Is this always correct?
    public static RedisString Of(byte[] bs) => new RedisString(Encoding.ASCII.GetString(bs));

    public static RedisArray Of(params RedisType[] elements) => new RedisArray(elements);

    public static RedisString Nil() => new RedisString(null);
}

// TODO(mlesniak) Should a RedisString not be a byte[] array without any encoding?
public class RedisString : RedisType
{
    public string? Value { get; set; } = null;

    public RedisString(string? value)
    {
        Value = value;
    }

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

public class RedisArray : RedisType
{
    public List<RedisType> Values { get; set; }

    public RedisArray(params RedisType[] elements)
    {
        Values = elements.ToList();
    }

    public override byte[] Serialize()
    {
        var sb = new StringBuilder();
        sb.Append($"*{Values!.Count}");
        sb.Append("\r\n");
        Values.ForEach(v =>
        {
            byte[] array = v.Serialize();
            sb.Append(Encoding.ASCII.GetString(array));
        });
        return Encoding.ASCII.GetBytes(sb.ToString());
    }
}
