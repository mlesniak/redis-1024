using System.Collections;
using System.Collections.Concurrent;

using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core;

public delegate void DatabaseUpdated();

public class Database : IDatabaseManagement, IDatabase
{
    private static readonly ILogger log = Logging.For<Database>();

    private readonly ConcurrentDictionary<string, DatabaseValue> _storage = new();
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ReaderWriterLockSlim _writeLock = new();
    public event DatabaseUpdated DatabaseUpdates;

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
        _writeLock.EnterReadLock();
        try
        {
            log.LogDebug("Setting {Key}", key);
            DateTime? expirationDate = expiration.HasValue
                ? DateTime.UtcNow.AddMilliseconds((double)expiration)
                : null;
            DatabaseValue dbValue = new(value, expirationDate);
            _storage[key] = dbValue;
            DatabaseUpdates.Invoke();
        }
        finally
        {
            _writeLock.ExitReadLock();
        }
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
        _writeLock.EnterReadLock();
        try
        {
            log.LogDebug("Removing {Key}", key);
            _storage.Remove(key, out DatabaseValue? _);
        }
        finally
        {
            _writeLock.ExitReadLock();
        }
    }

    public void WriteLock(Action action)
    {
        _writeLock.EnterWriteLock();
        try
        {
            action.Invoke();
        }
        finally
        {
            _writeLock.ExitWriteLock();
        }   
    }

    // internal
    public int Count
    {
        get => _storage.Count;
    }

    IEnumerator<KeyValuePair<string, DatabaseValue>> IEnumerable<KeyValuePair<string, DatabaseValue>>.GetEnumerator() =>
        _storage.GetEnumerator();

    public void Clear()
    {
        _storage.Clear();
    }

    public IEnumerator GetEnumerator()
    {
        return _storage.GetEnumerator();
    }
}