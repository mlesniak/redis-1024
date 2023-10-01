using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Jobs;

public class PersistenceJob : IJob
{
    private static readonly ILogger log = Logging.For<PersistenceJob>();
    private readonly IPersistenceProvider _persistenceProvider;
    private readonly IDatabaseManagement _database;
    private bool _dirty;
    private readonly Configuration.JobConfiguration _configuration;

    public PersistenceJob(Configuration configuration, IDatabaseManagement database, IPersistenceProvider persistenceProvider)
    {
        _database = database;
        _persistenceProvider = persistenceProvider;
        _configuration = configuration.PersistenceJob;
        database.DatabaseUpdates += DatabaseUpdated;
    }

    private void DatabaseUpdated()
    {
        log.LogDebug("Database updated, setting dirty flag");
        _dirty = true;
    }

    public async Task Start()
    {
        var delay = _configuration.Interval;
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
}