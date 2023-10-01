using Microsoft.Extensions.Configuration;

namespace Lesniak.Redis;

public class Configuration
{
    public string DatabaseName => "database.json";
    public int MaxReadBuffer { get; init; } = 1024 * 1024;
    public int Port { get; init; } = 6379;
    public string? Password { get; init; } = null;
    public JobConfiguration PersistenceJob { get; init; } = new();
    public JobConfiguration CleanupJob { get; init;  } = new();

    public class JobConfiguration
    {
        public TimeSpan Interval { get; init; } = TimeSpan.FromMinutes(1);
    }

    private static Configuration _singleton;
    private static IConfiguration _configuration;

    public Configuration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        _configuration = builder.Build();
        _configuration.Bind(this);
    }

    public static IConfiguration GetSection(string key) => _configuration.GetSection(key);

    public override string ToString()
    {
        return $"{nameof(MaxReadBuffer)}: {MaxReadBuffer}, {nameof(Port)}: {Port}";
    }
}