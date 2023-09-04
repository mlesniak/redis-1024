using System.Collections.Concurrent;

using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Storage;

// TODO(mlesniak) Add basic persistence.
public class Database
{
    private static readonly ILogger _logger = Logging.For<Database>();

    private readonly IDateTimeProvider _dateTimeProvider;

    private readonly ConcurrentDictionary<string, DatabaseValue> _memory = new();

    public Database(IDateTimeProvider dateTimeProvider, bool startBackgroundJobs = true)
    {
        _dateTimeProvider = dateTimeProvider;
        if (startBackgroundJobs)
        {
            _logger.LogInformation("Spawning background cleaning job");
            Task.Run(StartBackgroundCleanup);
        }
    }

    /// <summary>
    /// Returns number of stored items.
    ///
    /// Note that we return even expired items, which have
    /// not been cleaned up yet.
    /// </summary>
    /// <returns>Number of keys in the memory structure</returns>
    public int Count
    {
        get => _memory.Count;
    }

    private async Task StartBackgroundCleanup()
    {
        while (true)
        {
            // Run once a minute.
            await Task.Delay(1_000 * 60);
            int removed = RemoveExpiredKeys();
            _logger.LogDebug("Cleaned up. Removed {Removed} entries", removed);
        }
    }

    public int RemoveExpiredKeys()
    {
        var removed = 0;
        foreach (KeyValuePair<string, DatabaseValue> pair in _memory)
        {
            if (!pair.Value.Expired)
            {
                continue;
            }

            _memory.Remove(pair.Key, out _);
            removed++;
        }

        return removed;
    }

    public void Set(string key, byte[] value, int? expMs)
    {
        _logger.LogInformation("Storing {Key} with expiration {Expiration}", key, expMs);
        _memory[key] = new(_dateTimeProvider, value, expMs);
    }

    public byte[]? Get(string key) => _memory.TryGetValue(key, out DatabaseValue? value) ? value.Value : null;
}