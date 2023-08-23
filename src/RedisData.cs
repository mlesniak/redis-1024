using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Lesniak.Redis;

public class RedisData
{
    public RedisDataType Type { get; private set; }
    public string? BulkString { get; private set; }
    public List<RedisData>? ArrayValues { get; private set; }

    private delegate (RedisData, int) Parser(byte[] data, int offset);

    private static Dictionary<byte, Parser> parsers = new();

    static RedisData()
    {
        parsers.Add((byte)'$', (data, offset) =>
        {
            RedisData result = new() { Type = RedisDataType.BulkString };
            var lengthEnd = Array.IndexOf(data, (byte)'\r', offset);
            var length = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, lengthEnd - offset - 1));
            int stringStart = lengthEnd + 2;
            result.BulkString = Encoding.ASCII.GetString(data, stringStart, length);
            return (result, stringStart + length + 2);
        });
        parsers.Add((byte)'*', (data, offset) =>
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
        });
    }

    public static RedisData Parse(byte[] data) => Parse(data, 0).Item1;

    static (RedisData, int) Parse(byte[] data, int offset) => parsers[data[offset]].Invoke(data, offset);

    public override string ToString()
    {
        return "TODO";
    }
}