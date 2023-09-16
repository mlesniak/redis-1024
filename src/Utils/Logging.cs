using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Infrastructure;

public static class Logging
{
    private static readonly ILoggerFactory _factory;

    public static ILogger For<T>() => _factory.CreateLogger<T>();

    static Logging()
    {
        _factory = LoggerFactory.Create(builder =>
        {
            builder.AddConfiguration(Configuration.GetSection("Logging"));
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
        });
    }
}