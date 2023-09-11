using System.Collections;
using System.Collections.Concurrent;
using System.Text.Json;

using Lesniak.Redis.Core.Storage.Jobs;
using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Storage;

// TODO(mlesniak) Add DEL method and command?
public class InMemoryStorage : IDatabase, IStorage
{
    private static readonly ILogger _logger = Logging.For<InMemoryStorage>();

    private readonly IDateTimeProvider _dateTimeProvider;

    private bool _dirty;

    private ConcurrentDictionary<string, DatabaseValue> _memory = new();
    private readonly Configuration _configuration;

    public InMemoryStorage(Configuration configuration, IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        _configuration = configuration;
        LoadData();
        
        // TODO(mlesniak) later manually
        StartJobs();
    }
    
    public event IDatabase.DataChangedHandler DataChanged;

    // No need for any explicit configuration.
    public void StartJobs()
    {
        // Task.Run(PersistenceJob);
        PersistenceJob persistenceJob = new PersistenceJob(this);
        DataChanged += persistenceJob.DataChangedHandler;
        persistenceJob.Run(_configuration);
    }

    /// <summary>
    ///     Returns number of stored items.
    ///     Note that we return even expired items, which have
    ///     not been cleaned up yet.
    /// </summary>
    /// <returns>Number of keys in the memory structure</returns>
    public int Count => _memory.Count;

    // private async Task PersistenceJob()
    // {
    //     _logger.LogInformation("Spawning persistence job");
    //     while (true)
    //     {
    //         if (_dirty)
    //         {
    //             PersistData();
    //             _dirty = false;
    //         }
    //
    //         // TODO(mlesniak) Make this configurable
    //         await Task.Delay(1_000);
    //     }
    // }

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

    public void Set(string key, byte[] value, int? expirationMs)
    {
        _logger.LogInformation("Storing {Key} with expiration {Expiration}", key, expirationMs);
        DateTime? expirationDate = null;
        if (expirationMs != null)
        {
            expirationDate = DateTime.Now.AddMilliseconds((double)expirationMs);
        }

        _memory[key] = new DatabaseValue(value, expirationDate);
        DataChanged.Invoke();
    }

    public byte[]? Get(string key)
    {
        return _memory.TryGetValue(key, out DatabaseValue? value) ? value.Value : null;
    }

    public void Remove(string key)
    {
        _memory.Remove(key, out DatabaseValue _);
    }

    public IEnumerator GetEnumerator() => _memory.GetEnumerator();
    public void Load(Configuration configuration, IDatabase database)
    {
        throw new NotImplementedException();
    }

    public void Save(Configuration configuration, IDatabase database)
    {
        throw new NotImplementedException();
    }
}