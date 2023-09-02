using System.Text;

namespace Lesniak.Redis.Core.Model;

public static class RedisTypeParser
{
    private delegate (RedisType, int) Parser(byte[] data, int offset);

    private static readonly Dictionary<byte, Parser> Parsers = new();

    static RedisTypeParser()
    {
        Parsers.Add((byte)'$', ParseBulkString);
        Parsers.Add((byte)'*', ParseArray);
    }
    
    public static RedisType Parse(byte[] data) => Parse(data, 0).Item1;

    // TODO(mlesniak) Move this to types, they now how to serialize and deserialize themselves.
    private static (RedisType, int) ParseArray(byte[] data, int offset)
    {
        RedisArray result = new();
        var numElementsIndexEnd = Array.IndexOf(data, (byte)'\r', offset);
        var numElements =
            Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, numElementsIndexEnd - offset - 1));
        offset = numElementsIndexEnd + 2;
        for (var i = 0; i < numElements; i++)
        {
            (RedisType elem, int nextArrayOffset) = Parse(data, offset);
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

    private static (RedisType, int) Parse(byte[] data, int offset) => Parsers[data[offset]].Invoke(data, offset);
}