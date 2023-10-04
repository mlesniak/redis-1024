using System.Text;

using Lesniak.Redis.Communication.Network.Types;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Communication.Network.Types;

public class RedisNumberTest
{
    [Fact]
    public void Serializing_Number_works()
    {
        var value = RedisNumber.From(123);
        Equal(":123\r\n", value.ToAsciiString());
    }

    [Fact]
    public void Deserializing_Number_Works()
    {
        var bytes = ":123\r\n"u8.ToArray();
        var (value, next) = RedisValue.Deserialize<RedisNumber>(bytes, 0);
        Equal(123, value.Value);
        Equal(6, next);
    }
}