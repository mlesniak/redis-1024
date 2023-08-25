using System.Text;

namespace Lesniak.Redis;

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
                sb.Append($"${BulkString!.Length}");
                sb.Append("\r\n");
                sb.Append(BulkString);
                sb.Append("\r\n");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    public static RedisData of(string s) => new() { Type = RedisDataType.BulkString, BulkString = s };

    public static RedisData of(params RedisData[] arrayElements) =>
        new() { Type = RedisDataType.Array, ArrayValues = arrayElements.ToList() };
}