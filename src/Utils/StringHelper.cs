using System.Text;

namespace Lesniak.Redis.Utils;

public static class StringHelper
{
    public static byte[] ToRedisMessage(this string s)
    {
        var withNewlines = s.Replace("\n", "\r\n") + "\r\n";
        return Encoding.UTF8.GetBytes(withNewlines);
    }

}