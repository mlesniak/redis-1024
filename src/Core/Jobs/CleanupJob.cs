using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Jobs;

public class CleanupJob : IJob
{
    private static readonly ILogger log = Logging.For<CleanupJob>();
    private readonly Database _database;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CleanupJob(IDateTimeProvider dateTimeProvider, Database database)
    {
        _dateTimeProvider = dateTimeProvider;
        _database = database;
    }

    /// <summary>
    /// Removes all expired entries from the database.
    /// </summary>
    private void Run()
    {
        log.LogInformation("Starting cleanup job");
        foreach (KeyValuePair<string, Database.DatabaseValue> kv in _database)
        {
            var expirationDate = kv.Value.ExpirationDate;
            if (expirationDate == null || expirationDate > _dateTimeProvider.Now)
            {
                continue;
            }

            log.LogInformation("Cleaning up key {Key}", kv.Key);
            _database.Remove(kv.Key);
        }

        log.LogInformation("Finished cleanup job");
    }

    public async Task Start()
    {
        var delay = TimeSpan.FromMinutes(1);
        log.LogInformation($"Triggering cleanup job perdiodically every {delay}");
        while (true)
        {
            Run();
            await Task.Delay(delay);
        }
    }
}