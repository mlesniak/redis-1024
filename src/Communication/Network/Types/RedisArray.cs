using System.Collections;
using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

public record RedisArray(RedisValue[] Values) : RedisValue, IEnumerable<RedisValue>
{
    public const char Identifier = '*';

    public RedisValue this[int index] => Values[index];

    public IEnumerator<RedisValue> GetEnumerator()
    {
        return Values.ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    public static (RedisValue, int) Deserialize(byte[] data, int offset)
    {
        List<RedisValue> result = new();
        int numElementsIndexEnd = Array.IndexOf(data, (byte)'\r', offset);
        int numElements =
            Int32.Parse(Encoding.ASCII.GetString(data, offset + 1, numElementsIndexEnd - offset - 1));
        offset = numElementsIndexEnd + 2;
        for (int i = 0; i < numElements; i++)
        {
            (RedisValue elem, int nextArrayOffset) = Deserialize<RedisValue>(data, offset);
            result.Add(elem);
            offset = nextArrayOffset;
        }

        var array = new RedisArray(result.ToArray());
        return (array, offset);
    }

    public override byte[] Serialize()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"*{Values.Length}");
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


    public virtual bool Equals(RedisArray? other)
    {
        return Values.SequenceEqual(other.Values);
    }

    public override int GetHashCode() => Values.GetHashCode();
}
