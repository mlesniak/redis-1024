namespace Lesniak.Redis;

public interface IConfiguration
{
    string DatabaseName { get; }
    int MaxReadBuffer { get; }
    int Port { get; }
    string? Password { get; }
    JobConfiguration PersistenceJob { get; }
    JobConfiguration CleanupJob { get; }

    public class JobConfiguration
    {
        public TimeSpan Interval { get; init; } = TimeSpan.FromMinutes(1);
    }

}