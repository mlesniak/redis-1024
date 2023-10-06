using System.Text;

using Lesniak.Redis.Communication.Network.Types;

namespace Lesniak.Redis.Test;

public static class TestHelper
{
    public static string ToAsciiString(this RedisValue value)
    {
        return Encoding.ASCII.GetString(value.Serialize());
    }
}
