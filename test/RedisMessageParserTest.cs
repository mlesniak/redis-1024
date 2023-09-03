using Lesniak.Redis.Core.Model;
using Lesniak.Redis.Utils;

using static Xunit.Assert;

namespace Lesniak.Redis.Test;

public class RedisMessageParserTest
{
    [Fact]
    public void ToRedisMessage_ValidInput_ReturnsCorrectResult()
    {
        var message = """
                      $5
                      HELLO
                      """.ToRedisMessage();
        
        Equal("$5\r\nHELLO\r\n"u8.ToArray(), message);
    }
    
    [Fact]
    public void ToRedisMessage_SimpleBulkString_ReturnsCorrectResult()
    {
        var message = """
                      $5
                      HELLO
                      """.ToRedisMessage();
        
        var data = RedisType.Deserialize<RedisString>(message);
        Equal("HELLO", data.Value);
    }

    [Fact]
    public void ToRedisMessage_SimpleArray_ReturnsCorrectResult()
    {
        var message = """
                      *2
                      $5
                      HELLO
                      $3
                      FOO
                      """.ToRedisMessage();
        
        var array = RedisType.Deserialize<RedisArray>(message);
        
        IsType<RedisString>(array[0]);
        Equal("HELLO", ((RedisString)array[0]).Value);
        IsType<RedisString>(array[1]);
        Equal("FOO", ((RedisString)array[1]).Value);
    }
    
    [Fact]
    public void ToRedisMessage_NestedArray_ReturnsCorrectResult()
    {
        var message = """
                      *2
                      *2
                      $3
                      BAR
                      $5
                      HELLO
                      $3
                      FOO
                      """.ToRedisMessage();
        
        var array = RedisType.Deserialize<RedisArray>(message);
        
        IsType<RedisArray>(array[0]);
        RedisArray subarray = (RedisArray)array[0];
        Equal("BAR", ((RedisString)(subarray[0])).Value);
        Equal("HELLO", ((RedisString)(subarray[1])).Value);
        IsType<RedisString>(array[1]);
        Equal("FOO", ((RedisString)array[1]).Value);
    }
}