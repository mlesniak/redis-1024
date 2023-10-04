namespace Lesniak.Redis.Test.Core;

public class TestConfiguration : IConfiguration
{
    public string DatabaseName { get; set; } = "database.json";
    public int MaxReadBuffer { get; } = 1_024;
    public int Port { get; } = 6379;
    public string? Password { get; set; }
    public IConfiguration.JobConfiguration PersistenceJob { get; } = new() { Interval = TimeSpan.MaxValue };
    public IConfiguration.JobConfiguration CleanupJob { get; } = new() { Interval = TimeSpan.MaxValue };


}