using System.Text;

using static Xunit.Assert;

namespace Lesniak.Redis.Test;

public class RedisDataSerializationTest
{
    [Fact]
    public void ToRedisSerialization_BulkString_ReturnsValidInput()
    {
        var data = RedisData.of("HELLO");

        var serialized = data.ToRedisSerialization();

        Equal(
            ToByteArray("""
                        $5
                        HELLO
                        """),
            serialized);
    }

    [Fact]
    public void ToRedisMessage_SimpleArray_ReturnsCorrectResult()
    {
        var data = RedisData.of(
            RedisData.of("HELLO"),
            RedisData.of("FOO")
        );

        var serialized = data.ToRedisSerialization();

        Equal(
            ToByteArray("""
                        *2
                        $5
                        HELLO
                        $3
                        FOO
                        """),
            serialized);
    }

    [Fact]
    public void ToRedisMessage_NestedArray_ReturnsCorrectResult()
    {
        var data = RedisData.of(
            RedisData.of(
                RedisData.of("BAR"),
                RedisData.of("HELLO")
            ),
            RedisData.of("FOO")
        );

        var serialized = data.ToRedisSerialization();

        Equal(
            ToByteArray("""
                        *2
                        *2
                        $3
                        BAR
                        $5
                        HELLO
                        $3
                        FOO
                        """),
            serialized);
    }

    private byte[] ToByteArray(string s)
    {
        return Encoding.ASCII.GetBytes(s.Replace("\n", "\r\n") + "\r\n");
    }
}