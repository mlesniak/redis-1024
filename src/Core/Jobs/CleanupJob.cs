using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Jobs;

public class CleanupJob : IJob
{
    private static readonly ILogger log = Logging.For<CleanupJob>();
    private readonly IConfiguration.JobConfiguration _configurationCleanupJob;
    private readonly IDatabaseManagement _database;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CleanupJob(IConfiguration configuration, IDateTimeProvider dateTimeProvider, IDatabaseManagement database)
    {
        _configurationCleanupJob = configuration.CleanupJob;
        _dateTimeProvider = dateTimeProvider;
        _database = database;
    }

    public async Task Start()
    {
        TimeSpan delay = _configurationCleanupJob.Interval;
        log.LogInformation($"Starting cleanup job perdiodically every {delay}");
        while (true)
        {
            await Task.Delay(delay);
            Run();
        }
    }

    /// <summary>
    ///     Removes all expired entries from the database.
    /// </summary>
    private void Run()
    {
        log.LogInformation("Starting cleanup job");
        foreach (KeyValuePair<string, Database.DatabaseValue> kv in _database)
        {
            DateTime? expirationDate = kv.Value.ExpirationDate;
            if (expirationDate == null || expirationDate > _dateTimeProvider.Now)
            {
                continue;
            }

            log.LogInformation("Cleaning up key {Key}", kv.Key);
            _database.Remove(kv.Key);
        }

        log.LogInformation("Finished cleanup job");
    }
}