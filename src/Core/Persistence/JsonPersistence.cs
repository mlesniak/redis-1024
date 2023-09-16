using System.Collections.Concurrent;
using System.Text.Json;

using Lesniak.Redis.Core.Jobs;
using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

using static Lesniak.Redis.Core.Database;

namespace Lesniak.Redis.Core.Persistence;

public class JsonPersistence : IPersistenceProvider
{
    private static readonly ILogger log = Logging.For<PersistenceJob>();
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDatabaseManagement _database;

    public JsonPersistence(IDateTimeProvider dateTimeProvider, IDatabaseManagement database)
    {
        _dateTimeProvider = dateTimeProvider;
        _database = database;
    }

    public void Save()
    {
        // To have a clean separation between the internal representation and the
        // persistence, we do not allow access to the storage internals. Instead,
        // we retrieve all values manually. This is not the most performant way,
        // but sufficient for this example.
        // TODO(mlesniak) Locking while copying values.
        ConcurrentDictionary<string, DatabaseValue> values = new();
        foreach (KeyValuePair<string, DatabaseValue> kv in _database)
        {
            values[kv.Key] = kv.Value;
        }

        // Serialize as JSON and write to disk.
        log.LogInformation("Persisting database to disk");
        var json = JsonSerializer.Serialize(values);
        // TODO(mlesniak) Externalize filename via configuration file.
        File.WriteAllText("database.json", json);
    }

    public void Load()
    {
        log.LogInformation("Loading database from disk");

        // Clean database.
        foreach (KeyValuePair<string, DatabaseValue> kv in _database)
        {
            _database.Remove(kv.Key);
        }

        ConcurrentDictionary<string, DatabaseValue> values = new();
        var json = File.ReadAllText("database.json");
        var dict = JsonSerializer.Deserialize<Dictionary<string, DatabaseValue>>(json);
        foreach (KeyValuePair<string, DatabaseValue> kv in dict)
        {
            DatabaseValue dbValue = kv.Value;
            int? expiration = null;
            if (dbValue.ExpirationDate != null)
            {
                var x = (_dateTimeProvider.Now - dbValue.ExpirationDate!);
                expiration = x.Value.Milliseconds;
            }

            _database.Set(kv.Key, dbValue.Value!, expiration);
        }
    }
}