using Lesniak.Redis.Utils;

namespace Lesniak.Redis.Test.Core;

/// <summary>
/// A dateTimeProvider which allows to manually advance time.
/// Since we are only interested in the relative passing of
/// time, we do not provide a mechanism to set the start date
/// itself.
/// </summary>
public class TestClock : IClock
{
    public TestClock()
    {
        Now = DateTime.UtcNow;
    }

    public TestClock(string currentTime)
    {
        Now = DateTime.Parse(currentTime);
    }

    public DateTime Now { get; private set; }

    public void Add(int ms)
    {
        Now = Now.AddMilliseconds(ms);
    }
}