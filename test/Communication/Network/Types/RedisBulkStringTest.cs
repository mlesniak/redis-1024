using System.Text;

using Lesniak.Redis.Communication.Network.Types;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Communication.Network.Types;

// TODO(mlesniak) tests for parsing errors
public class RedisBulkStringTest
{
    [Fact]
    public void Serializing_BulkString_works()
    {
        var value = RedisBulkString.From("test");
        Equal("$4\r\ntest\r\n", value.ToAsciiString());
    }

    [Fact]
    public void Deserializing_BulkString_works()
    {
        var bytes = "$5\r\nHello"u8.ToArray();
        var (value, next) = RedisValue.Deserialize<RedisBulkString>(bytes, 0);
        Equal("Hello", value.AsciiValue);
        Equal(11, next);
    }

    [Fact]
    public void Serialize_Null_String()
    {
        var value = RedisBulkString.Nil();
        Equal("$-1\r\n", value.ToAsciiString());
    }

    [Fact]
    public void Convert_Bytes_To_BulkString()
    {
        var bytes = "hello"u8.ToArray();
        var value = RedisBulkString.From(bytes);
        Equal("hello", value.AsciiValue);
    }

    [Fact]
    public void Parsing_Error_Throws_Correct_Exception()
    {
         
        var bytes = "$3\r\nHello"u8.ToArray();
        var (value, next) = RedisValue.Deserialize<RedisBulkString>(bytes, 0);
    }
}
