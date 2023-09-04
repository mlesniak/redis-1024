using Lesniak.Redis.Storage;

namespace Lesniak.Redis.Test.Utils;

public class TestDateTimeProvider : IDateTimeProvider
{
    private DateTime _now;

    public void SetFixedDate(DateTime now)
    {
        _now = now;
    }

    public void AddMilliseconds(long ms)
    {
        _now = _now.AddMilliseconds(ms);
    }

    public DateTime Now
    {
        get
        {
            return _now;
        }
    }
}