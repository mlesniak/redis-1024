using Microsoft.Extensions.Configuration;

namespace Lesniak.Redis;

public class Configuration : IConfiguration
{
    private static Configuration _singleton;
    private static Microsoft.Extensions.Configuration.IConfiguration _configuration;

    public Configuration()
    {
        // Use default values for logging configuration in case we run
        // a test. Until now, we did not need to configure logging in
        // test, hence we did not add it to the IConfiguration interface.
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "Logging:LogLevel:Lesniak.Redis", "Debug" }
        });
        Logging = configurationBuilder.Build();

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
    public IConfiguration.JobConfiguration PersistenceJob { get; init; } = new();
    public IConfiguration.JobConfiguration CleanupJob { get; init; } = new();

    public Microsoft.Extensions.Configuration.IConfiguration Logging { get; private set; } 

    public override string ToString()
    {
        return $"{nameof(MaxReadBuffer)}: {MaxReadBuffer}, {nameof(Port)}: {Port}";
    }
}