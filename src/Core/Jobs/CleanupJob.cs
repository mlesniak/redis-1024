using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Jobs;

public class CleanupJob : IJob
{
    private readonly ILogger<CleanupJob> _log;
    private readonly IConfiguration.JobConfiguration _configurationCleanupJob;
    private readonly IDatabaseManagement _database;
    private readonly IClock _clock;

    public CleanupJob(
        ILogger<CleanupJob> log,
        IConfiguration configuration,
        IClock clock,
        IDatabaseManagement database)
    {
        _log = log;
        _configurationCleanupJob = configuration.CleanupJob;
        _clock = clock;
        _database = database;
    }

    public async Task Start()
    {
        TimeSpan delay = _configurationCleanupJob.Interval;
        _log.LogInformation($"Starting cleanup job perdiodically every {delay}");
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
        _log.LogInformation("Starting cleanup job");
        foreach (KeyValuePair<string, Database.DatabaseValue> kv in _database)
        {
            DateTime? expirationDate = kv.Value.ExpirationDate;
            if (expirationDate == null || expirationDate > _clock.Now)
            {
                continue;
            }

            _log.LogInformation("Cleaning up key {Key}", kv.Key);
            _database.Remove(kv.Key);
        }

        _log.LogInformation("Finished cleanup job");
    }
}
