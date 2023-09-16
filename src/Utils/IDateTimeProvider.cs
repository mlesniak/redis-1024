namespace Lesniak.Redis.Infrastructure;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}