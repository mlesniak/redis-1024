using System.Text;

namespace Lesniak.Redis.Core.Model;

// TODO(mlesniak) Index and IEnumerable
public class RedisArray : RedisType
{
    public List<RedisType> Values { get; set; }

    public RedisType this[int index]
    {
        get { return Values[index]; }
        set { Values[index] = value; }
    }

    private RedisArray(params RedisType[] elements)
    {
        Values = elements.ToList();
    }

    public static RedisArray From(params RedisType[] elements) => new(elements);

    public static new (RedisType, int) Deserialize(byte[] data, int offset)
    {
        RedisArray result = new();
        var numElementsIndexEnd = Array.IndexOf(data, (byte)'\r', offset);
        var numElements =
            Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, numElementsIndexEnd - offset - 1));
        offset = numElementsIndexEnd + 2;
        for (var i = 0; i < numElements; i++)
        {
            (RedisType elem, int nextArrayOffset) = RedisType.Deserialize(data, offset);
            result.Values.Add(elem);
            offset = nextArrayOffset;
        }

        return (result, offset);
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