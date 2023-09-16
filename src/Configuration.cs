using Microsoft.Extensions.Configuration;

namespace Lesniak.Redis;

public class Configuration
{
    public int MaxReadBuffer { get; init; } = 1024 * 1024;
    public int Port { get; init; } = 6379;
    public string DatabaseName { get; init; } = "database.json";
    public JobConfiguration PersistenceJob { get; init; } = new();
    public JobConfiguration CleanupJob { get; init; } = new();

    public class JobConfiguration
    {
        public TimeSpan Interval { get; init; } = TimeSpan.FromMinutes(1);
    }

    private static Configuration _singleton;
    private static IConfiguration _configuration;

    static Configuration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        _configuration = builder.Build();
        _singleton = _configuration.Get<Configuration>() ?? new Configuration();
    }

    public static Configuration Get() => _singleton;

    public static IConfiguration GetSection(string key) => _configuration.GetSection(key);

    public override string ToString()
    {
        return $"{nameof(MaxReadBuffer)}: {MaxReadBuffer}, {nameof(Port)}: {Port}";
    }
}