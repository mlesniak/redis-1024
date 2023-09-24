using System.Collections;
using System.Collections.Concurrent;

using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core;

public delegate void DatabaseUpdated();

// ReSharper disable once RedundantExtendsListEntry
public class Database : IDatabaseManagement, IDatabase
{
    private static readonly ILogger log = Logging.For<Database>();

    // -----
    // TODO(mlesniak) unsubscribe from everything?
    // TODO(mlesniak) error handling on publish
    public delegate void AsyncMessageReceiver(string channel, byte[] message);

    private readonly ConcurrentDictionary<string, List<AsyncMessageReceiver>> _subscriptions = new();

    public void Publish(string channel, byte[] message)
    {
        if (!_subscriptions.TryGetValue(channel, out List<AsyncMessageReceiver>? receivers))
        {
            return;
        }

        foreach (AsyncMessageReceiver receiver in receivers)
        {
            receiver.Invoke(channel, message);
        }
    }

    public void Subscribe(string channel, AsyncMessageReceiver receiver)
    {
        _subscriptions.AddOrUpdate(channel,
            new List<AsyncMessageReceiver> { receiver },
            (_, current) =>
            {
                // TODO(mlesniak) concurrency-safe list
                current.Add(receiver);

                return current;
            }
        );
    }

    public void Unsubscribe(string channel, AsyncMessageReceiver receiver)
    {
        if (!_subscriptions.TryGetValue(channel, out List<AsyncMessageReceiver>? receivers))
        {
            return;
        }

        receivers.Remove(receiver);
    }
    // -----

    private readonly ConcurrentDictionary<string, DatabaseValue> _storage = new();

    private readonly IDateTimeProvider _dateTimeProvider;

    // We use a ReaderWriterLockSlim to allow multiple writers of 
    // a single value to be processed in parallel (since we use a 
    // thread-safe data structure). At the same time, we want to 
    // prevent any single-element writes from interrupting the
    // periodic persistence job, which is why we use a write lock
    // for that. Hence, the name is a bit misleading.
    private readonly ReaderWriterLockSlim _writeLock = new();
    public event DatabaseUpdated? DatabaseUpdates;

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
            DatabaseUpdates?.Invoke();
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