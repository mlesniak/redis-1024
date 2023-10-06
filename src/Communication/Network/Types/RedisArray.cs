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

    public RedisValue this[int index] => Values[index];

    public IEnumerator<RedisValue> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    public static (RedisValue, int) Deserialize(byte[] data, int offset)
    {
        RedisArray result = new();
        int numElementsIndexEnd = Array.IndexOf(data, (byte)'\r', offset);
        int numElements =
            Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, numElementsIndexEnd - offset - 1));
        offset = numElementsIndexEnd + 2;
        for (int i = 0; i < numElements; i++)
        {
            (RedisValue elem, int nextArrayOffset) = Deserialize<RedisValue>(data, offset);
            result.Values.Add(elem);
            offset = nextArrayOffset;
        }

        return (result, offset);
    }

    public override byte[] Serialize()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"*{Values!.Count}");
        sb.Append("\r\n");
        foreach (RedisValue value in Values)
        {
            byte[] array = value.Serialize();
            sb.Append(Encoding.ASCII.GetString(array));
        }
        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    public static RedisArray From(params RedisValue[] elements)
    {
        return new RedisArray(elements);
    }
}
