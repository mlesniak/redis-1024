using System.Collections.Concurrent;
using System.Text.Json;

using Lesniak.Redis.Core.Jobs;
using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

using static Lesniak.Redis.Core.Database;

namespace Lesniak.Redis.Core.Persistence;

public class JsonPersistence : IPersistenceProvider
{
    private readonly ILogger<JsonPersistence> _log;
    private readonly IDatabaseManagement _database;
    private readonly string _databaseName;
    private readonly IClock _clock;

    public JsonPersistence(
        ILogger<JsonPersistence> log,
        IConfiguration configuration,
        IClock clock,
        IDatabaseManagement database)
    {
        _log = log;
        _clock = clock;
        _database = database;
        _databaseName = configuration.DatabaseName;
    }

    public void Save()
    {
        // To have a clean separation between the internal representation and the
        // persistence, we do not allow access to the storage internals. Instead,
        // we retrieve all values manually. This is not the most performant way,
        // but sufficient for this example.
        //
        // We do not lock writes while retrieving the data, so we might miss some
        // values which are added while we create our internal data structure.
        //
        // This is fine for our playground example, but needs a better solution
        // in a real-world application.
        ConcurrentDictionary<string, DatabaseValue> values = new();
        foreach (KeyValuePair<string, DatabaseValue> kv in _database)
        {
            values[kv.Key] = kv.Value;
        }

        // Serialize as JSON and write to disk.
        _log.LogInformation("Persisting database to disk");
        string json = JsonSerializer.Serialize(values);
        File.WriteAllText(_databaseName, json);
    }

    public void Load()
    {
        if (!File.Exists(_databaseName))
        {
            _log.LogInformation("No database file found, skipping loading");
            return;
        }
        _log.LogInformation("Loading database from disk");
        _database.Clear();

        string json = File.ReadAllText(_databaseName);
        Dictionary<string, DatabaseValue>? dict = JsonSerializer.Deserialize<Dictionary<string, DatabaseValue>>(json)!;
        foreach (KeyValuePair<string, DatabaseValue> kv in dict)
        {
            DatabaseValue dbValue = kv.Value;
            int? expiration = null;
            if (dbValue.ExpirationDate != null)
            {
                TimeSpan? x = _clock.Now - dbValue.ExpirationDate!;
                expiration = x.Value.Milliseconds;
            }

            _database.Set(kv.Key, dbValue.Value!, expiration);
        }
    }
}
