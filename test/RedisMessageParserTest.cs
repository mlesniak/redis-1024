using Lesniak.Redis;

namespace test;

public class RedisMessageParserTest
{
    [Fact]
    public void TestBulkStringParsing()
    {
        var message = "$5\r\nHELLO\r\n"u8.ToArray();
        var data = RedisData.Parse(message);

        Assert.Equal(RedisData.DataType.BulkString, data.Type);
        Assert.Equal("HELLO", data.BulkString);
    }
}