using System.Text;

namespace Lesniak.Redis.Model;

public class RedisData
{
    public RedisDataType Type { get; set; }
    public string? BulkString { get; set; }
    public List<RedisData>? ArrayValues { get; set; }

    public override string ToString() => Encoding.ASCII.GetString(ToRedisSerialization());

    public byte[] ToRedisSerialization()
    {
        var sb = new StringBuilder();

        switch (Type)
        {
            case RedisDataType.Array:
                sb.Append($"*{ArrayValues!.Count}");
                sb.Append("\r\n");
                ArrayValues.ForEach(v =>
                {
                    byte[] array = v.ToRedisSerialization();
                    sb.Append(Encoding.ASCII.GetString(array));
                });
                break;
            case RedisDataType.BulkString:
                // Handle nil responses, which are modeled as bulk
                // strings with a negative length.
                if (BulkString == null)
                {
                    sb.Append("$-1");
                }
                else
                {
                    sb.Append($"${BulkString!.Length}");
                    sb.Append("\r\n");
                    sb.Append(BulkString);
                }
                sb.Append("\r\n");

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    public static RedisData of(string s) => new() { Type = RedisDataType.BulkString, BulkString = s };

    // Is this always correct?
    public static RedisData of(byte[] bs) =>
        new() { Type = RedisDataType.BulkString, BulkString = Encoding.ASCII.GetString(bs) };


    public static RedisData of(params RedisData[] arrayElements) =>
        new() { Type = RedisDataType.Array, ArrayValues = arrayElements.ToList() };

    public static RedisData nil() => new() { Type = RedisDataType.BulkString };
}