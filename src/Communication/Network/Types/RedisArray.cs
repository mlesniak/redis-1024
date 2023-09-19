using System.Collections;
using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

public class RedisArray : RedisType, IEnumerable<RedisType>
{
    public const char Identifier = '*';

    private RedisArray(params RedisType[] elements)
    {
        Values = elements.ToList();
    }

    public IList<RedisType> Values { get; }

    public static (RedisType, int) Deserialize(byte[] data, int offset)
    {
        RedisArray result = new();
        var numElementsIndexEnd = Array.IndexOf(data, (byte)'\r', offset);
        var numElements =
            Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, numElementsIndexEnd - offset - 1));
        offset = numElementsIndexEnd + 2;
        for (var i = 0; i < numElements; i++)
        {
            (RedisType elem, int nextArrayOffset) = RedisType.Deserialize<RedisType>(data, offset);
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
        foreach (RedisType value in Values)
        {
            byte[] array = value.Serialize();
            sb.Append(Encoding.ASCII.GetString(array));
        }
        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    public static RedisArray From(params RedisType[] elements) => new(elements);

    public RedisType this[int index]
    {
        get { return Values[index]; }
    }

    public IEnumerator<RedisType> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Values.GetEnumerator();
    }
}