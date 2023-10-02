using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Test.Core;

public class TestLogger<T> : ILogger<T>
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // Ignored.
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // Basically ignored.
        return false;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState: notnull
    {
        // Ignored.
        return null;
    }

    public static ILogger<T> Get()
    {
        return new TestLogger<T>();
    }
}