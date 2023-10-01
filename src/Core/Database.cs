using System.Collections;
using System.Collections.Concurrent;

using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core;

public delegate void DatabaseUpdated();

// ReSharper disable once RedundantExtendsListEntry
public class Database : IDatabaseManagement, IDatabase
{
    public delegate void AsyncMessageReceiver(string channel, byte[] message);

    private static readonly ILogger log = Logging.For<Database>();
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ConcurrentDictionary<string, DatabaseValue> _storage = new();

    private readonly ConcurrentDictionary<string, List<Tuple<string, AsyncMessageReceiver>>> _subscriptions = new();

    // We use a ReaderWriterLockSlim to allow multiple writers of 
    // a single value to be processed in parallel (since we use a 
    // thread-safe data structure). At the same time, we want to 
    // prevent any single-element writes from interrupting the
    // periodic persistence job, which is why we use a write lock
    // for that. Hence, the name is a bit misleading.
    private readonly ReaderWriterLockSlim _writeLock = new();
    private string? _password;

    public Database(IConfiguration configuration, IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        _password = configuration.Password;
    }

    // internal
    public int Count => _storage.Count;

    public event DatabaseUpdated? DatabaseUpdates;

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
        if (!_subscriptions.TryGetValue(channel, out List<Tuple<string, AsyncMessageReceiver>>? receivers))
        {
            return 0;
        }

        for (int i = receivers.Count - 1; i >= 0; i--)
        {
            AsyncMessageReceiver receiver = receivers[i].Item2;
            try
            {
                receiver.Invoke(channel, message);
            }
            catch (Exception e)
            {
                receivers.RemoveAt(i);
            }
        }
        return receivers.Count;
    }

    public int Subscribe(string clientId, string channel, AsyncMessageReceiver receiver)
    {
        log.LogDebug("{ClientId}: Adding subscription to {Channel}", clientId, channel);
        Tuple<string, AsyncMessageReceiver> tuple = Tuple.Create(clientId, receiver);
        _subscriptions.AddOrUpdate(channel,
            new List<Tuple<string, AsyncMessageReceiver>> { tuple },
            (_, current) =>
            {
                lock (current)
                {
                    current.Add(tuple);
                }

                return current;
            }
        );

        if (_subscriptions.TryGetValue(channel, out List<Tuple<string, AsyncMessageReceiver>>? receivers))
        {
            return receivers.Count;
        }

        log.LogWarning("Unable to get info about recently subscribed {Channel}", channel);
        return 1;
    }

    public IEnumerable<string> UnsubscribeAll(string clientId)
    {
        return _subscriptions
            .Select(pair =>
            {
                log.LogInformation("Examining channel {Channel}", pair.Key);
                if (pair.Value.RemoveAll(x => x.Item1 == clientId) > 0)
                {
                    log.LogDebug("For {ClientId}: Removing subscription to {Channel}", clientId, pair.Key);
                    return pair.Key;
                }
                return null;
            })
            .Where(k => k != null)
            .ToList() as List<string>;
    }

    public void Unsubscribe(string clientId, string channel)
    {
        if (!_subscriptions.TryGetValue(channel, out List<Tuple<string, AsyncMessageReceiver>>? receivers))
        {
            return;
        }
        lock (receivers)
        {
            receivers.RemoveAll(x => x.Item1 == clientId);
        }
    }

    public void SetPassword(string password)
    {
        _password = password;
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