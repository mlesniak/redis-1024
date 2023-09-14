using System.Collections;
using System.Collections.Concurrent;

using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core;

public class Database : IEnumerable<KeyValuePair<string, Database.DatabaseValue>>
{
    private static readonly ILogger log = Logging.For<Database>();

    private readonly ConcurrentDictionary<string, DatabaseValue> _storage = new();
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly object _writeLock = new();

    public Database(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public class DatabaseValue
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
        log.LogDebug("Retrieving {Key}", key);
        _storage.TryGetValue(key, out DatabaseValue? dbValue);
        if (dbValue == null || dbValue.ExpirationDate <= _dateTimeProvider.Now)
        {
            return null;
        }

        return dbValue.Value;
    }

    public void Remove(string key)
    {
        log.LogDebug("Removing {Key}", key);
        _storage.Remove(key, out DatabaseValue? _);
    }

    // internal
    public int Count
    {
        get => _storage.Count;
    }

    public void LockWrites(Action operation)
    {
        lock (_writeLock)
        {
            operation();
        }
    }

    IEnumerator<KeyValuePair<string, DatabaseValue>> IEnumerable<KeyValuePair<string, DatabaseValue>>.GetEnumerator() =>
        _storage.GetEnumerator();

    public IEnumerator GetEnumerator()
    {
        return _storage.GetEnumerator();
    }
}