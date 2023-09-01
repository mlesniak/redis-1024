using System.Text;

using Microsoft.Extensions.Primitives;

namespace Lesniak.Redis.Core.Model;

public abstract class RedisType
{
    public abstract byte[] Serialize();

    // TODO(mlesniak) Strange design decision?
    public override string ToString() =>
        Encoding.ASCII.GetString(Serialize());
    
    public static RedisString of(string? s) => new RedisString(s);

    // TODO(mlesniak) Is this always correct?
    public static RedisString of(byte[] bs) => new RedisString(Encoding.ASCII.GetString(bs));

    public static RedisArray of(params RedisType[] elements) => new RedisArray(elements);

    public static RedisString nil() => new RedisString(null);
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

// public class RedisData
// {
//     // TODO(mlesniak) Use inheritance
//     public RedisDataType Type { get; set; }
//     public string? BulkString { get; set; }
//     public List<RedisData>? ArrayValues { get; set; }
//
//     public override string ToString() => Encoding.ASCII.GetString(ToRedisSerialization());
//
//     public byte[] ToRedisSerialization()
//     {
//         var sb = new StringBuilder();
//
//         switch (Type)
//         {
//             case RedisDataType.Array:
//                 sb.Append($"*{ArrayValues!.Count}");
//                 sb.Append("\r\n");
//                 ArrayValues.ForEach(v =>
//                 {
//                     byte[] array = v.ToRedisSerialization();
//                     sb.Append(Encoding.ASCII.GetString(array));
//                 });
//                 break;
//             case RedisDataType.BulkString:
//                 // Handle nil responses, which are modeled as bulk
//                 // strings with a negative length.
//                 if (BulkString == null)
//                 {
//                     sb.Append("$-1");
//                 }
//                 else
//                 {
//                     sb.Append($"${BulkString!.Length}");
//                     sb.Append("\r\n");
//                     sb.Append(BulkString);
//                 }
//
//                 sb.Append("\r\n");
//
//                 break;
//             default:
//                 throw new ArgumentOutOfRangeException();
//         }
//
//         return Encoding.ASCII.GetBytes(sb.ToString());
//     }
// }