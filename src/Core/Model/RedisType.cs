using System.Text;

namespace Lesniak.Redis.Core.Model;

public abstract class RedisType
{
    protected static (RedisType, int) Deserialize(byte[] data, int offset)
    {
        byte identifier = data[offset];
        Func<byte[], int, (RedisType, int)> method = (char)identifier switch
        {
            RedisArray.Identifier => RedisArray.Deserialize,
            RedisString.Identifier => RedisString.Deserialize,
            _ => throw new ArgumentOutOfRangeException(
                nameof(identifier),
                $"Unknown identifier byte {identifier}")
        };
        return method(data, offset);
    }

    public static T Deserialize<T>(byte[] data) where T : RedisType => (T)Deserialize(data, 0).Item1;

    public abstract byte[] Serialize();
    
    // For the time being, we use the serialized representation to present
    // a readable form of this type.
    public override string ToString() => Encoding.ASCII.GetString(Serialize());
}