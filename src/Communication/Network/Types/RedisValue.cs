using System.Text;

namespace Lesniak.Redis.Communication.Network.Types;

/// <summary>
///     Generic type for all Redis types, forcing them to implement both
///     serialization and deserialization methods.
/// </summary>
public abstract record RedisValue
{
    public abstract byte[] Serialize();

    /// <summary>
    ///     This method is used to deserialize a byte array into a RedisType.
    ///     Note that we do not want to use an abstract method here, as we want
    ///     clients to be able to call a generic and static method to parse
    ///     arbitrary byte streams.
    /// </summary>
    /// <param name="data">The complete data stream</param>
    /// <param name="offset">The position to start parsing with</param>
    /// <returns>
    ///     A tuple containing the deserialized RedisType and the position in
    ///     the stream where the deserialization ended and the next deserialization
    ///     can start, e.g. for array elements.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown in case the identifier byte is unknown.
    /// </exception>
    public static (T, int) Deserialize<T>(byte[] data, int offset) where T: RedisValue
    {
        byte identifier = data[offset];
        Func<byte[], int, (RedisValue, int)> method = (char)identifier switch
        {
            RedisArray.Identifier => RedisArray.Deserialize,
            RedisBulkString.Identifier => RedisBulkString.Deserialize,
            RedisNumber.Identifier => RedisNumber.Deserialize,
            _ => throw new ArgumentOutOfRangeException(
                nameof(identifier),
                $"Unknown identifier byte {identifier}")
        };
        return ((T, int))method(data, offset);
    }

    // For the time being, we use the serialized representation to present
    // a readable form of this type.
    public override string ToString()
    {
        return Encoding.ASCII.GetString(Serialize());
    }
}
