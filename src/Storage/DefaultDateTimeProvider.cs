namespace Lesniak.Redis.Storage;

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime Now
    {
        get => DateTime.Now;
    }
}
