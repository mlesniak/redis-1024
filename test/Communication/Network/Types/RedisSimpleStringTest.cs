using System.Text;

using Lesniak.Redis.Communication.Network.Types;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Communication.Network.Types;

public class RedisSimpleStringTest
{
    [Fact]
    public void Serializing_SimpleString_works()
    {
        var value = RedisSimpleString.From("test");
        Equal("+test\r\n", value.ToAsciiString());
    }
}