using System.Text;

using Lesniak.Redis.Communication.Network.Types;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Communication.Network.Types;

// TODO(mlesniak) tests for parsing errors
// TODO(mlesniak) Rename all test methods: given_when_then_every_underscroe
// TODO(mlesniak) expected vs actual
// TODO(mlesniak) introduce assertion library / .net talk
public class RedisArrayTest
{
    // [Fact]
    // public void Serializing_BulkString_works()
    // {
    //     var value = RedisBulkString.From("test");
    //     Equal("$4\r\ntest\r\n", value.ToAsciiString());
    // }
    //
    // [Fact]
    // public void Deserializing_BulkString_works()
    // {
    //     var bytes = "$5\r\nHello"u8.ToArray();
    //     var (value, next) = RedisValue.Deserialize<RedisBulkString>(bytes, 0);
    //     Equal("Hello", value.AsciiValue);
    //     Equal(11, next);
    // }
    //
    // [Fact]
    // public void Serialize_Null_String()
    // {
    //     var value = RedisBulkString.Nil();
    //     Equal("$-1\r\n", value.ToAsciiString());
    // }
    //
    // [Fact]
    // public void Convert_BytesTo_BulkString()
    // {
    //     var bytes = "hello"u8.ToArray();
    //     var value = RedisBulkString.From(bytes);
    //     Equal("hello", value.AsciiValue);
    // }
}
