using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Utils;

public static class Logging
{
    private static readonly ILoggerFactory _factory;

    public static ILogger For<T>() => _factory.CreateLogger<T>();

    static Logging()
    {
        _factory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Lesniak.Redis", LogLevel.Debug)
                .AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
        });
    }
}