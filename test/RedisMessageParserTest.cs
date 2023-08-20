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
    public void TestBulkStringParsing()
    {
        var message = """
                      $5
                      HELLO
                      """.ToRedisMessage();
        
        var data = RedisData.Parse(message);

        Equal(RedisData.DataType.BulkString, data.Type);
        Equal("HELLO", data.BulkString);
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
        
        var data = RedisData.Parse(message);
        
        Equal(RedisData.DataType.Array, data.Type);
        
        Equal(RedisData.DataType.BulkString, data.ArrayValues![0].Type);
        Equal("HELLO", data.ArrayValues![0].BulkString);
        
        Equal(RedisData.DataType.BulkString, data.ArrayValues![1].Type);
        Equal("FOO", data.ArrayValues![1].BulkString);
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
        
        var data = RedisData.Parse(message);
        
        Equal(RedisData.DataType.Array, data.Type);
        
        Equal(RedisData.DataType.Array, data.ArrayValues![0].Type);
        var array = data.ArrayValues![0];
        Equal("BAR", array.ArrayValues?[0].BulkString);
        Equal("HELLO", array.ArrayValues?[1].BulkString);
        
        Equal(RedisData.DataType.BulkString, data.ArrayValues![1].Type);
        Equal("FOO", data.ArrayValues?[1].BulkString);
    }

}