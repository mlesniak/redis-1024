using System.Collections.Concurrent;

using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core;

public class Database
{
    private static readonly ILogger log = Logging.For<Database>();

    private readonly ConcurrentDictionary<string, DatabaseValue> _storage = new();
    private readonly IDateTimeProvider _dateTimeProvider;

    public Database(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    private class DatabaseValue
    {
        public byte[]? Value { get; }
        public DateTime? ExpirationDate { get; }

        public DatabaseValue(byte[]? value, DateTime? expirationDate)
        {
            Value = value;
            ExpirationDate = expirationDate;
        }
    }

    public void Set(string key, byte[] value, long? expiration = null)
    {
        log.LogDebug("Setting {Key}", key);
        DateTime? expirationDate = expiration.HasValue
            ? DateTime.UtcNow.AddMilliseconds((double)expiration)
            : null;
        DatabaseValue dbValue = new(value, expirationDate);
        _storage[key] = dbValue;
    }

    public byte[]? Get(string key)
    {
        // TODO(mlesniak) Check for expiration date
        log.LogDebug("Retrieving {Key}", key);
        return _storage.TryGetValue(key, out DatabaseValue? dbValue) ? dbValue.Value : null;
    }

    public void Remove(string key)
    {
        log.LogDebug("Removing {Key}", key);
        _storage.Remove(key, out DatabaseValue? _);
    }
}