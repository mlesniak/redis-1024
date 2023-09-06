namespace Lesniak.Redis.Storage;

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime Now
    {
        // TODO(mlesniak) Use DateTimeOffset for proper UTC handling?
        get => DateTime.Now;
    }
}
