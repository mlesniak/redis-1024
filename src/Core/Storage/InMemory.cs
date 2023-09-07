using System.Collections.Concurrent;
using System.Text.Json;

using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Storage;

public class InMemory
{
    private static readonly ILogger _logger = Logging.For<InMemory>();

    private readonly IDateTimeProvider _dateTimeProvider;

    private bool _dirty;

    private ConcurrentDictionary<string, DatabaseValue> _memory = new();

    public InMemory(IDateTimeProvider dateTimeProvider, bool startBackgroundJobs = true)
    {
        _dateTimeProvider = dateTimeProvider;
        LoadData();

        if (startBackgroundJobs)
        {
            Task.Run(MemoryCleanupJob);
            Task.Run(PersistenceJob);
        }
    }

    /// <summary>
    ///     Returns number of stored items.
    ///     Note that we return even expired items, which have
    ///     not been cleaned up yet.
    /// </summary>
    /// <returns>Number of keys in the memory structure</returns>
    public int Count => _memory.Count;

    private async Task PersistenceJob()
    {
        _logger.LogInformation("Spawning persistence job");
        while (true)
        {
            if (_dirty)
            {
                PersistData();
                _dirty = false;
            }

            // TODO(mlesniak) Make this configurable
            await Task.Delay(1_000);
        }
    }

    // For now, we persist as a simple JSON file. Is it realistic
    // to parse the original files given our lines of code limitations?
    private void PersistData()
    {
        // TODO(mlesniak) abstractions via interfaces and/or namespaces.
        // TODO(mlesniak) We're breaking Sepeation of Concerns here.
        _logger.LogInformation("Persisting data");
        JsonSerializerOptions options = new();
        options.Converters.Add(new DatabaseValueConverter());
        string json = JsonSerializer.Serialize(_memory, options);
        File.WriteAllText("output.json", json);
    }

    private void LoadData()
    {
        _logger.LogInformation("Loading stored data");
        string json = File.ReadAllText("output.json");
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.Converters.Add(new DatabaseValueConverter());
        // options.IncludeFields = true;
        _memory = JsonSerializer.Deserialize<ConcurrentDictionary<string, DatabaseValue>>(
            json,
            options
        ) ?? throw new InvalidDataException();
        _logger.LogInformation($"Loaded {_memory.Count} entries");
    }

    private async Task MemoryCleanupJob()
    {
        _logger.LogInformation("Spawning background cleaning job");
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
        int removed = 0;
        foreach (KeyValuePair<string, DatabaseValue> pair in _memory)
        {
            if (!KeyExpired(pair.Value))
            {
                continue;
            }

            _memory.Remove(pair.Key, out _);
            removed++;
        }

        return removed;
    }

    private bool KeyExpired(DatabaseValue value)
    {
        return value.Expiration != null && _dateTimeProvider.Now < value.Expiration;
    }

    public void Set(string key, byte[] value, int? expirationMs)
    {
        _logger.LogInformation("Storing {Key} with expiration {Expiration}", key, expirationMs);
        _dirty = true;
        DateTime? expirationDate = null;
        if (expirationMs != null)
        {
            expirationDate = DateTime.Now.AddMilliseconds((double)expirationMs);
        }

        _memory[key] = new DatabaseValue(value, expirationDate);
    }

    public byte[]? Get(string key)
    {
        return _memory.TryGetValue(key, out DatabaseValue? value) ? value.Value : null;
    }

    // TODO(mlesniak) Add DEL method and command?
}