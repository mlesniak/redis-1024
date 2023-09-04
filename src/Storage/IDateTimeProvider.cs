namespace Lesniak.Redis.Storage;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}
