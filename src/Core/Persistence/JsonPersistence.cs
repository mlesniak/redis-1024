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
    private readonly string _databaseName;

    public JsonPersistence(IDateTimeProvider dateTimeProvider, IDatabaseManagement database)
    {
        _dateTimeProvider = dateTimeProvider;
        _database = database;
        _databaseName = Configuration.Get().DatabaseName;
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
        log.LogInformation("Persisting database to disk");
        var json = JsonSerializer.Serialize(values);
        File.WriteAllText(_databaseName, json);
    }

    public void Load()
    {
        if (!File.Exists(_databaseName))
        {
            log.LogInformation("No database file found, skipping loading");
            return;
        }
        log.LogInformation("Loading database from disk");
        _database.Clear();

        var json = File.ReadAllText(_databaseName);
        var dict = JsonSerializer.Deserialize<Dictionary<string, DatabaseValue>>(json)!;
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