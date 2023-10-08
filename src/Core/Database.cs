using System.Collections;
using System.Collections.Concurrent;

using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core;

public delegate void DatabaseUpdated();

// ReSharper disable once RedundantExtendsListEntry
public class Database : IDatabaseManagement, IDatabase
{
    public delegate void AsyncMessageReceiver(string channel, byte[] message);

    private readonly ILogger<Database> _log;
    private readonly IClock _clock;
    private readonly ConcurrentDictionary<string, DatabaseValue> _storage = new();

    // Structure from channel -> (clientId -> receiver function).
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, AsyncMessageReceiver>> _subscriptions =
        new();

    // We use a ReaderWriterLockSlim to allow multiple writers of 
    // a single value to be processed in parallel (since we use a 
    // thread-safe data structure). At the same time, we want to 
    // prevent any single-element writes from interrupting the
    // periodic persistence job, which is why we use a write lock
    // for that. Hence, the name is a bit misleading.
    private readonly ReaderWriterLockSlim _writeLock = new();
    private string? _password;

    public Database(ILogger<Database> log, IConfiguration configuration, IClock clock)
    {
        _log = log;
        _clock = clock;
        _password = configuration.Password;
    }

    public event DatabaseUpdated? DatabaseUpdates;

    public void Set(string key, byte[] value, TimeSpan? expiration = null)
    {
        _writeLock.EnterReadLock();
        try
        {
            _log.LogDebug("Setting {Key}", key);
            DateTime? expirationDate = null;
            if (expiration.HasValue)
            {
                expirationDate = _clock.Now.Add(expiration.Value);
            }
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
        _log.LogDebug("Retrieving {Key}", key);
        _storage.TryGetValue(key, out DatabaseValue? dbValue);
        if (dbValue == null || dbValue.ExpirationDate <= _clock.Now)
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
            _log.LogDebug("Removing {Key}", key);
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

    IEnumerator<KeyValuePair<string, DatabaseValue>> IEnumerable<KeyValuePair<string, DatabaseValue>>.GetEnumerator()
    {
        return _storage.GetEnumerator();
    }

    public void Clear()
    {
        _storage.Clear();
    }

    public IEnumerator GetEnumerator()
    {
        return _storage.GetEnumerator();
    }

    public int Publish(string channel, byte[] message)
    {
        if (!_subscriptions.TryGetValue(channel, out ConcurrentDictionary<string, AsyncMessageReceiver>? receivers))
        {
            return 0;
        }

        foreach (var pair in receivers)
        {
            try
            {
                pair.Value.Invoke(channel, message);
            }
            catch (Exception e)
            {
                _log.LogWarning("Handler threw exception {Exception}", e.Message);
                receivers.TryRemove(pair.Key, out _);
            }
        }

        return receivers.Count;
    }

    public int Subscribe(string clientId, string channel, AsyncMessageReceiver receiver)
    {
        _log.LogDebug("{ClientId}: Adding subscription to {Channel}", clientId, channel);
        ConcurrentDictionary<string, AsyncMessageReceiver> initialValue = new(
            new[]
            {
                new KeyValuePair<string, AsyncMessageReceiver>(clientId, receiver)
            });
        _subscriptions.AddOrUpdate(channel,
            initialValue,
            (_, current) =>
            {
                current.TryAdd(clientId, receiver);
                return current;
            }
        );

        _subscriptions.TryGetValue(channel, out ConcurrentDictionary<string, AsyncMessageReceiver>? receivers);
        return receivers.Count;
    }

    public IEnumerable<string> UnsubscribeAll(string clientId)
    {
        return _subscriptions
            .Select(pair =>
            {
                _log.LogInformation("Examining channel {Channel}", pair.Key);
                pair.Value.TryRemove(clientId, out _);
                _log.LogDebug("For {ClientId}: Removing subscription to {Channel}", clientId, pair.Key);
                return pair.Key;
            })
            .Where(k => k != null)
            .ToList() as List<string>;
    }

    public void Unsubscribe(string clientId, string channel)
    {
        if (!_subscriptions.TryGetValue(channel, out ConcurrentDictionary<string, AsyncMessageReceiver>? receivers))
        {
            return;
        }
        lock (receivers)
        {
            receivers.Remove(clientId, out _);
        }
    }

    public bool VerifyPassword(string password)
    {
        if (_password == null)
        {
            return true;
        }
        return _password == password;
    }

    public bool AuthenticationRequired => _password != null;

    public class DatabaseValue
    {
        public DatabaseValue(byte[]? value, DateTime? expirationDate)
        {
            Value = value;
            ExpirationDate = expirationDate;
        }

        public byte[]? Value { get; }
        public DateTime? ExpirationDate { get; }
    }
}
