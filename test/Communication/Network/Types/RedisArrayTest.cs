using System.Text;

using Lesniak.Redis.Communication.Network.Types;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Communication.Network.Types;

public class RedisArrayTest
{
    [Fact]
    public void Serializing_Basic_Arrays()
    {
        var actual = RedisArray.From(
            RedisBulkString.From("Hello"),
            RedisNumber.From(1));
        Equal("*2\r\n$5\r\nHello\r\n:1\r\n", actual.ToAsciiString());
    }

    [Fact]
    public void Serialize_Nested_Array()
    {
        var actual = RedisArray.From(
            RedisBulkString.From("Hello"),
            RedisArray.From(
                RedisArray.From(
                    RedisBulkString.From("World"),
                    RedisNumber.From(1)
                ),
                RedisBulkString.From("Michael")));
        Equal(
            "*2\r\n$5\r\nHello\r\n*2\r\n*2\r\n$5\r\nWorld\r\n:1\r\n$7\r\nMichael\r\n",
            actual.ToAsciiString());
    }

    [Fact]
    public void Deserialize_Basic_Array()
    {
        var (array, next) = RedisValue.Deserialize<RedisArray>(
            "*2\r\n$5\r\nHello\r\n:1\r\n"u8.ToArray(), 0);
        var expectedParsedValues = RedisArray.From(
            RedisBulkString.From("Hello"),
            RedisNumber.From(1));
        Equal(expectedParsedValues, array);
        Equal(19, next);
    }


    [Fact]
    public void Deserialize_Nested_Array()
    {
        var (array, next) = RedisValue.Deserialize<RedisArray>(
            "*2\r\n$5\r\nHello\r\n*2\r\n*2\r\n$5\r\nWorld\r\n:1\r\n$7\r\nMichael\r\n"u8.ToArray(), 0);
        var expectedParsedValues = RedisArray.From(
            RedisBulkString.From("Hello"),
            RedisArray.From(
                RedisArray.From(
                    RedisBulkString.From("World"),
                    RedisNumber.From(1)
                ),
                RedisBulkString.From("Michael")));
        Equal(expectedParsedValues, array.Values);
        Equal(51, next);
    }
}
