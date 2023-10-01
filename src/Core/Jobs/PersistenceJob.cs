using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Jobs;

public class PersistenceJob : IJob
{
    private static readonly ILogger log = Logging.For<PersistenceJob>();
    private readonly Configuration.JobConfiguration _configuration;
    private readonly IDatabaseManagement _database;
    private readonly IPersistenceProvider _persistenceProvider;
    private bool _dirty;

    public PersistenceJob(Configuration configuration, IDatabaseManagement database,
        IPersistenceProvider persistenceProvider)
    {
        _database = database;
        _persistenceProvider = persistenceProvider;
        _configuration = configuration.PersistenceJob;
        database.DatabaseUpdates += DatabaseUpdated;
    }

    public async Task Start()
    {
        TimeSpan delay = _configuration.Interval;
        log.LogInformation("Starting persistence job, checking for write every {Delay}", delay);
        while (true)
        {
            if (_dirty)
            {
                _database.WriteLock(() =>
                {
                    _persistenceProvider.Save();
                    _dirty = false;
                });
            }

            await Task.Delay(delay);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private void DatabaseUpdated()
    {
        log.LogDebug("Database updated, setting dirty flag");
        _dirty = true;
    }
}