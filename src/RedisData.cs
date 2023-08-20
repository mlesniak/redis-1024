using System.Text;

namespace Lesniak.Redis;

public static class DataTypeIdentifier
{
    public static char Identifier(this RedisData.DataType type)
    {
        return type switch
        {
            RedisData.DataType.Array => '*',
            RedisData.DataType.BulkString => '$',
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public class RedisData
{
    public enum DataType
    {
        Array,
        BulkString,
    }

    public DataType Type { get; set; }
    public string? BulkString { get; set; }
    public List<RedisData>? ArrayValues { get; set; }

    public static RedisData Parse(byte[] data)
    {
        (RedisData result, _) = Parse(data, 0);
        return result;
    }

    // return (parsed data, beginning of next element)
    static (RedisData, int) Parse(byte[] data, int offset)
    {
        RedisData result = new();
        int nextOffset;

        switch (data[offset])
        {
            case (byte)'$':
                result.Type = DataType.BulkString;
                var lengthEnd = Array.IndexOf(data, (byte)'\r', offset);
                var length = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, lengthEnd - offset - 1));
                int stringStart = lengthEnd + 2;
                result.BulkString = Encoding.ASCII.GetString(data, stringStart, length);
                nextOffset = stringStart + length + 2;
                break;
            case (byte)'*':
                result.Type = DataType.Array;
                result.ArrayValues = new();
                var numElementsIndexEnd = Array.IndexOf(data, (byte)'\r', offset);
                var numElements =
                    Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, numElementsIndexEnd - offset - 1));
                offset = numElementsIndexEnd + 2;
                for (var i = 0; i < numElements; i++)
                {
                    (RedisData elem, int nextArrayOffset) = Parse(data, offset);
                    result.ArrayValues.Add(elem);
                    offset = nextArrayOffset;
                }

                nextOffset = offset;
                break;
            default:
                throw new ArgumentException($"Invalid byte {data[offset]} to parse");
        }

        return (result, nextOffset);
    }

    public override string ToString()
    {
        return "TODO";
    }
}