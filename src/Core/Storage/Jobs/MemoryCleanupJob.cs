using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Storage.Jobs;

public class MemoryCleanupJob
{
    private static readonly ILogger _logger = Logging.For<MemoryCleanupJob>();
    private IDatabase _database;
    private IDateTimeProvider _dateTimeProvider;

    public void Run(
        Configuration configuration,
        IDateTimeProvider dateTimeProvider,
        IDatabase database)
    {
        _database = database;
        _dateTimeProvider = dateTimeProvider;
        // TODO(mlesniak) check configuration if job is enabled.
        Task.Run(Run);
    }

    async Task Run()
    {
        _logger.LogInformation("Spawning background cleanup job");
        while (true)
        {
            // Run once a minute.
            await Task.Delay(1_000 * 60);
            int removed = RemoveExpiredKeys();
            _logger.LogDebug("Cleaned up. Removed {Removed} entries", removed);
        }
    }

    int RemoveExpiredKeys()
    {
        // use _database 

        int removed = 0;
        foreach (KeyValuePair<string, DatabaseValue> kv in _database)
        {
            if (!kv.Value.Expired(_dateTimeProvider.Now))
            {
                continue;
            }

            _database.Remove(kv.Key);
            removed++;
        }

        return removed;
    }
}