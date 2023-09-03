using System.Text;

namespace Lesniak.Redis.Core.Model;

public abstract class RedisType
{
    // TODO(mlesniak) use Func instead of delegate Func<byte[], int, (RedisType, int)>
    protected delegate (RedisType, int) Deserializer(byte[] data, int offset);

    public static RedisType Deserialize(byte[] data) => Deserialize(data, 0).Item1;

    // TODO(mlesniak) Comment about registry
    private static Deserializer GetDeserializer(byte identifier)
    {
        return (char)identifier switch
        {
            '*' => RedisArray.Deserialize,
            '$' => RedisString.Deserialize,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static (RedisType, int) Deserialize(byte[] data, int offset)
    {
        Deserializer method = GetDeserializer(data[offset]);
        return method(data, offset);
    }


    // For the time being, we use the serialized representation to present
    // a readable form of this type.
    public override string ToString() => Encoding.ASCII.GetString(Serialize());
    
    public abstract byte[] Serialize();
}