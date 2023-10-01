using Microsoft.Extensions.Configuration;

namespace Lesniak.Redis;

public class Configuration
{
    private static Configuration _singleton;
    private static IConfiguration _configuration;

    public Configuration()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, false);
        _configuration = builder.Build();
        _configuration.Bind(this);
    }

    public string DatabaseName => "database.json";
    public int MaxReadBuffer { get; init; } = 1024 * 1024;
    public int Port { get; init; } = 6379;
    public string? Password { get; init; } = null;
    public JobConfiguration PersistenceJob { get; init; } = new();
    public JobConfiguration CleanupJob { get; init; } = new();

    public static IConfiguration GetSection(string key)
    {
        return _configuration.GetSection(key);
    }

    public override string ToString()
    {
        return $"{nameof(MaxReadBuffer)}: {MaxReadBuffer}, {nameof(Port)}: {Port}";
    }

    public class JobConfiguration
    {
        public TimeSpan Interval { get; init; } = TimeSpan.FromMinutes(1);
    }
}