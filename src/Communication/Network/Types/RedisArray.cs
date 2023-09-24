using System.Collections;
using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

public class RedisArray : RedisValue, IEnumerable<RedisValue>
{
    public const char Identifier = '*';

    private RedisArray(params RedisValue[] elements)
    {
        Values = elements.ToList();
    }

    public IList<RedisValue> Values { get; }

    public static (RedisValue, int) Deserialize(byte[] data, int offset)
    {
        RedisArray result = new();
        var numElementsIndexEnd = Array.IndexOf(data, (byte)'\r', offset);
        var numElements =
            Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, numElementsIndexEnd - offset - 1));
        offset = numElementsIndexEnd + 2;
        for (var i = 0; i < numElements; i++)
        {
            (RedisValue elem, int nextArrayOffset) = RedisValue.Deserialize<RedisValue>(data, offset);
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
        foreach (RedisValue value in Values)
        {
            byte[] array = value.Serialize();
            sb.Append(Encoding.ASCII.GetString(array));
        }
        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    public static RedisArray From(params RedisValue[] elements) => new(elements);

    public RedisValue this[int index]
    {
        get { return Values[index]; }
    }

    public IEnumerator<RedisValue> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Values.GetEnumerator();
    }
}