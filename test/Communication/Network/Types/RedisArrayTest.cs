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

    [Fact]
    public void Verify_enumeration_works()
    {
        var array = RedisArray.From(
            RedisNumber.From(3),
            RedisNumber.From(2),
            RedisNumber.From(1));
        var list = new List<long>();
        
        foreach(var redisValue in array)
        {
            var v = (RedisNumber)redisValue;
            list.Add(v.Value);
        }

        Equal(new List<long> { 3, 2, 1 }, list);
    }
    
    [Fact]
    public void Verify_indexing_works()
    {
        var array = RedisArray.From(
            RedisNumber.From(3),
            RedisNumber.From(2),
            RedisNumber.From(1));
        var list = new List<long>();

        Equal(3, ((array[0] as RedisNumber)!).Value);
        Equal(2, ((array[1] as RedisNumber)!).Value);
        Equal(1, ((array[2] as RedisNumber)!).Value);
    }
}
