using System.Collections.Concurrent;
using System.Text.Json;

using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Jobs;

public class PersistenceJob : IJob
{
    private readonly Database _database;
    private static readonly ILogger log = Logging.For<PersistenceJob>();
    private bool _dirty;

    public PersistenceJob(Database database)
    {
        _database = database;
        database.DatabaseUpdates += DatabaseUpdated;
    }

    private void DatabaseUpdated()
    {
        log.LogDebug("Database updated, setting dirty flag");
        _dirty = true;
    }

    public async Task Start()
    {
        var delay = TimeSpan.FromSeconds(5);
        log.LogInformation("Starting persistence job, checking for write every {Delay}", delay);
        while (true)
        {
            if (_dirty)
            {
                // TODO(mlesniak) add locking
                Persist();
                _dirty = false;
            }
            await Task.Delay(delay);
        }
    }

    // TODO(mlesniak) Externalize this to allow loading as well.
    private void Persist()
    {
        // To have a clean separation between the internal representation and the
        // persistence, we do not allow access to the storage internals. Instead,
        // we retrieve all values manually. This is not the most performant way,
        // but sufficient for this example.
        ConcurrentDictionary<string, Database.DatabaseValue> values = new();
        foreach (KeyValuePair<string, Database.DatabaseValue> kv in _database)
        {
            values[kv.Key] = kv.Value;
        }

        // Serialize as JSON and write to disk.
        log.LogInformation("Persisting database to disk");
        var json = JsonSerializer.Serialize(values);
        // TODO(mlesniak) Externalize filename via configuration file.
        File.WriteAllText("database.json", json);
    }
}