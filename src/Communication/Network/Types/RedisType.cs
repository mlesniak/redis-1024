using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

/// <summary>
/// Generic type for all Redis types, forcing them to implement both
/// serialization and deserialization methods.
/// </summary>
public abstract class RedisType
{
    public static T Deserialize<T>(byte[] data) where T : RedisType => (T)Deserialize(data, 0).Item1;

    public abstract byte[] Serialize();

    /// <summary>
    /// This method is used to deserialize a byte array into a RedisType.
    /// </summary>
    /// <param name="data">The complete data stream</param>
    /// <param name="offset">The position to start parsing with</param>
    /// <returns>
    /// A tuple containing the deserialized RedisType and the position in
    /// the stream where the deserialization ended and the next deserialization
    /// can start, e.g. for array elements.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown in case the identifier byte is unknown.
    /// </exception>
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

    // For the time being, we use the serialized representation to present
    // a readable form of this type.
    public override string ToString() => Encoding.ASCII.GetString(Serialize());
}