using System.Collections.Concurrent;
using System.Text.Json;

using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Storage;

// TODO(mlesniak) Add basic persistence.
public class Database
{
    private static readonly ILogger _logger = Logging.For<Database>();

    private readonly IDateTimeProvider _dateTimeProvider;

    private ConcurrentDictionary<string, DatabaseValue> _memory = new();

    private bool _dirty = false;

    public Database(IDateTimeProvider dateTimeProvider, bool startBackgroundJobs = true)
    {
        LoadData();
        
        _dateTimeProvider = dateTimeProvider;
        if (startBackgroundJobs)
        {
            Task.Run(MemoryCleanupJob);
            Task.Run(PersistenceJob);
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

            // TODO(mlesniak) Configurable
            await Task.Delay(1_000);
        }
    }

    // For now, we persist as a simple JSON file. Is it realistic
    // to parse the original files given our lines of code limitations?
    private void PersistData()
    {
        // TODO(mlesniak) abstractions via interfaces and/or namespaces.
        _logger.LogInformation("Persisting data");
        JsonSerializerOptions options = new();
        options.Converters.Add(new DatabaseValueConverter());
        string json = JsonSerializer.Serialize(_memory, options);
        File.WriteAllText("output.json", json);
    }

    private void LoadData()
    {
        _logger.LogInformation("Loading stored data");
        // var json = File.ReadAllText("output.json");
        // var options = new JsonSerializerOptions();
        // options.IncludeFields = true;
        // _memory = JsonSerializer.Deserialize<ConcurrentDictionary<string, DatabaseValue>>(
        //     json, 
        //     options
        // );
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
        _dirty = true;
    }

    public byte[]? Get(string key) => _memory.TryGetValue(key, out DatabaseValue? value) ? value.Value : null;
    
    // TODO(mlesniak) Add DEL option?
}