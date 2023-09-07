namespace Lesniak.Redis.Core.Storage;

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime Now =>
        // TODO(mlesniak) Use DateTimeOffset for proper UTC handling?
        DateTime.Now;
}