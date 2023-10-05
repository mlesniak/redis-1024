using System.Text;

using Lesniak.Redis.Communication.Network.Types;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Communication.Network.Types;

public class RedisBulkStringTest
{
    [Fact]
    public void Serializing_BulkString_works()
    {
        var value = RedisBulkString.From("test");
        Equal("$4\r\ntest\r\n", TestHelper.ToAsciiString(value));
    }

    // TODO(mlesniak) deserialize
    [Fact]
    public void Deserializing_BulkString_works()
    {
        
    }
}