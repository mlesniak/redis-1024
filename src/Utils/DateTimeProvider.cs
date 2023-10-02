namespace Lesniak.Redis.Utils;

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}