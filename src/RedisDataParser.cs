using System.Text;

namespace Lesniak.Redis;

public static class RedisDataParser
{
    private delegate (RedisData, int) Parser(byte[] data, int offset);

    private static readonly Dictionary<byte, Parser> Parsers = new();

    static RedisDataParser()
    {
        Parsers.Add((byte)'$', ParseBulkString);
        Parsers.Add((byte)'*', ParseArray);
    }
    
    public static RedisData Parse(byte[] data) => Parse(data, 0).Item1;

    private static (RedisData, int) ParseArray(byte[] data, int offset)
    {
        RedisData result = new() { Type = RedisDataType.Array, ArrayValues = new() };
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

        return (result, offset);
    }

    private static (RedisData, int) ParseBulkString(byte[] data, int offset)
    {
        RedisData result = new() { Type = RedisDataType.BulkString };
        var lengthEnd = Array.IndexOf(data, (byte)'\r', offset);
        var length = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, lengthEnd - offset - 1));
        int stringStart = lengthEnd + 2;
        result.BulkString = Encoding.ASCII.GetString(data, stringStart, length);
        return (result, stringStart + length + 2);
    }

    private static (RedisData, int) Parse(byte[] data, int offset) => Parsers[data[offset]].Invoke(data, offset);
}