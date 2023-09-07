namespace Lesniak.Redis.Core.Storage;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}