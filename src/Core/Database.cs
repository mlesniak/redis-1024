using System.Collections.Concurrent;

using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core;

public class Database
{
    private static ILogger logger = Logging.For<Database>();

    private ConcurrentDictionary<string, DatabaseValue> _storage = new();

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
        logger.LogDebug("Setting {Key}", key);
        DateTime? expirationDate = expiration.HasValue
            ? DateTime.UtcNow.AddMilliseconds((double)expiration)
            : null;
        DatabaseValue dbValue = new(value, expirationDate);
        _storage[key] = dbValue;
    }

    public byte[]? Get(string key)
    {
        logger.LogDebug("Retrieving {Key}", key);
        return _storage.TryGetValue(key, out DatabaseValue? dbValue) ? dbValue.Value : null;
    }

    public void Remove(string key)
    {
        logger.LogDebug("Removing {Key}", key);
        _storage.Remove(key, out DatabaseValue? _);
    }
}