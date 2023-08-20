using System.Text;

namespace Lesniak.Redis;

public class RedisData
{
    public enum DataType
    {
        Array,
        BulkString,
    }

    public DataType Type { get; set; }
    public string? BulkString { get; set; } = null;
    public List<RedisData>? ArrayValues { get; set; } = null;

    public static RedisData Parse(byte[] data)
    {
        var (result, _) = Parse(data, 0);
        return result;
    }

    // return (parsed data, beginning of next element)
    static (RedisData, int) Parse(byte[] data, int offset)
    {
        RedisData result = new();
        int end = 0;
        
        if (data[offset] == '*')
        {
            result.Type = DataType.Array;
            result.ArrayValues = new();
            var numEndIndex = Array.IndexOf(data, (byte)'\r', offset);
            // TODO(mlesniak) expectation, count vs endIndex
            var num = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, numEndIndex - offset - 1));
            offset = numEndIndex + 2;
            for (var i = 0; i < num; i++)
            {
                var (elem, end2) = Parse(data, offset);
                result.ArrayValues.Add(elem);
                offset = end2;
            }
            end = offset;
        }
        else if (data[offset] == '$')
        {
            result.Type = DataType.BulkString;
            var lenEnd = Array.IndexOf(data, (byte)'\r');
            var len = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, lenEnd - 1));
            int start = offset + lenEnd + 2;
            result.BulkString = Encoding.ASCII.GetString(data, start, len);
            end = start + len + 2;
        }
        else
        {
            throw new ArgumentException($"Invalid byte {data[offset]}");
        }

        return (result, end);
    }

    public override string ToString()
    {
        return "TODO"; 
    }
    
}