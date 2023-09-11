using Microsoft.Extensions.Configuration;

namespace Lesniak.Redis;

// TODO(mlesniak) Allow override for tests?
public class Configuration
{
    public int MaxReadBuffer { get; set; } = 1024 * 1024;
    public int Port { get; set; } = 6379;

    public static Configuration Load()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        var config = builder.Build().Get<Configuration>();
        return config ?? new Configuration();
    }

    public override string ToString()
    {
        return $"{nameof(MaxReadBuffer)}: {MaxReadBuffer}";
    }
}