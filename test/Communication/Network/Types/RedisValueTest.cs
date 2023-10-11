using System.Text;

using Lesniak.Redis.Communication.Network.Types;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Communication.Network.Types;

public class RedisValueTest
{
    [Fact]
    public void Exception_thrown_when_unknown_identifier()
    {
        var bytes = "?something"u8.ToArray();
        Throws<ArgumentException>(() =>
        {
            RedisValue.Deserialize<RedisNumber>(bytes);
        });
    }
}
