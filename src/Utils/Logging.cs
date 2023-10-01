using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Infrastructure;

public static class Logging
{
    private static readonly ILoggerFactory _factory;

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

    public static ILogger For<T>()
    {
        return _factory.CreateLogger<T>();
    }
}