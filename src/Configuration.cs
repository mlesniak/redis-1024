using Microsoft.Extensions.Configuration;

namespace Lesniak.Redis;

public class Configuration
{
    public int MaxReadBuffer { get; init; } = 1024 * 1024;
    public int Port { get; init; } = 6379;

    private static Configuration _singleton;

    static Configuration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        var config = builder.Build().Get<Configuration>();
        _singleton = config ?? new Configuration();
        
    }

    public static Configuration Get() => _singleton;
}