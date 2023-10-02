namespace Lesniak.Redis.Utils;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}