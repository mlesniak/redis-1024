using System.Text;

namespace Lesniak.Redis;

public static class StringHelper
{
    public static byte[] ToRedisMessage(this string s)
    {
        // On Unix like systems, we already have \r\n.
        // On Windows, we would need to replace \r with
        // \r\n, but we ignore that for now.
        var withNewlines = s + "\r\n";
        return Encoding.UTF8.GetBytes(withNewlines);
    }

}