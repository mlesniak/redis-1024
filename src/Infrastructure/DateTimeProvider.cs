namespace Lesniak.Redis.Infrastructure;

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}