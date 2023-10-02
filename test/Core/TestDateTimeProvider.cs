using Lesniak.Redis.Infrastructure;

namespace Lesniak.Redis.Test.Core;

/// <summary>
/// A dateTimeProvider which allows to manually advance time.
/// Since we are only interested in the relative passing of
/// time, we do not provide a mechanism to set the start date
/// itself.
/// </summary>
public class TestDateTimeProvider : IDateTimeProvider
{
    public void Add(int ms)
    {
        Now = Now.AddMilliseconds(ms);
    }

    public DateTime Now { get; private set; } = DateTime.UtcNow;
}