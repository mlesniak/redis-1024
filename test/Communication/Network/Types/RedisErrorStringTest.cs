using System.Text;

using Lesniak.Redis.Communication.Network.Types;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Communication.Network.Types;

public class RedisErrorStringTest
{
    [Fact]
    public void Serializing_ErrorString_works()
    {
        var value = RedisErrorString.From("test");
        Equal("-test\r\n", value.ToAsciiString());
    }
}
