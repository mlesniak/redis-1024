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
    public Array? ArrayValues { get; set; } = null;

    public static RedisData Parse(byte[] data, int offset = 0)
    {
        RedisData result = new();
        
        if (data[offset] == '*')
        {
            result.Type = DataType.Array;
            
        }
        else if (data[offset] == '$')
        {
            result.Type = DataType.BulkString;
            var lenEnd = Array.IndexOf(data, (byte)'\r');
            var len = Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, lenEnd - 1));
            int start = offset + lenEnd + 2;
            result.BulkString = Encoding.ASCII.GetString(data, start, len);
        }
        else
        {
            throw new ArgumentException($"Invalid byte {data[offset]}");
        }

        return result;
    }

    public override string ToString()
    {
        return "TODO"; 
    }
}