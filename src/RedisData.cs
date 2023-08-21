using System.Reflection;
using System.Text;

namespace Lesniak.Redis;


public class RedisData
{
    public RedisDataType Type { get; private set; }
    public string? BulkString { get; private set; }
    public List<RedisData>? ArrayValues { get; private set; }

    private delegate (RedisData, int) Parser(byte[] data, int offset);

    private static Dictionary<char, Parser> parsers = new();

    public static RedisData Parse(byte[] data)
    {
        (RedisData result, _) = Parse(data, 0);
        return result;
    }

    static (RedisData, int) Parse(byte[] data, int offset)
    {
        RedisData result = new();
        int nextOffset;

        switch (data[offset])
        {
            case var b when b == RedisDataType.BulkString.Identifier():
                result.Type = RedisDataType.BulkString;
                var lengthEnd = Array.IndexOf(data, (byte)'\r', offset);
                var length = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, lengthEnd - offset - 1));
                int stringStart = lengthEnd + 2;
                result.BulkString = Encoding.ASCII.GetString(data, stringStart, length);
                nextOffset = stringStart + length + 2;
                break;
            case var b when b == RedisDataType.Array.Identifier():
                result.Type = RedisDataType.Array;
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