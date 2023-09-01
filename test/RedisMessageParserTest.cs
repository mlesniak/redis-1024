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
        
        var data = RedisTypeParser.Parse(message);

        IsType<RedisString>(data);
        Equal("HELLO", ((RedisString)data).Value);
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
        
        var data = RedisTypeParser.Parse(message);
        
        IsType<RedisArray>(data);
        RedisArray array = (RedisArray)data;
        IsType<RedisString>(array.Values[0]);
        Equal("HELLO", ((RedisString)array.Values[0]).Value);
        IsType<RedisString>(array.Values[1]);
        Equal("FOO", ((RedisString)array.Values[1]).Value);
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
        
        var data = RedisTypeParser.Parse(message);
        
        
        IsType<RedisArray>(data);
        RedisArray array = (RedisArray)data;
        
        IsType<RedisArray>(array.Values[0]);
        RedisArray subarray = (RedisArray)array.Values[0];
        Equal("BAR", ((RedisString)(subarray.Values[0])).Value);
        Equal("HELLO", ((RedisString)(subarray.Values[1])).Value);
        IsType<RedisString>(array.Values[1]);
        Equal("FOO", ((RedisString)array.Values[1]).Value);
    }
}