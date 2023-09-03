using System.Text;

using Lesniak.Redis.Core.Model;

using static Xunit.Assert;

namespace Lesniak.Redis.Test;

public class RedisTypeSerializationTest
{
    [Fact]
    public void Serialize_BulkString_ReturnsValidInput()
    {
        var data = RedisString.From("HELLO");

        var serialized = data.Serialize();

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
        var data = RedisArray.From(
            RedisString.From("HELLO"),
            RedisString.From("FOO")
        );

        var serialized = data.Serialize();

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
        var data = RedisArray.From(
            RedisArray.From(
                RedisString.From("BAR"),
                RedisString.From("HELLO")
            ),
            RedisString.From("FOO")
        );

        var serialized = data.Serialize();

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