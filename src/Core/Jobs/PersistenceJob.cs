using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Jobs;

public class PersistenceJob : IJob
{
    private readonly ILogger<PersistenceJob> _log;
    private readonly IConfiguration.JobConfiguration _configuration;
    private readonly IDatabaseManagement _database;
    private readonly IPersistenceProvider _persistenceProvider;
    private bool _dirty;

    public PersistenceJob(
        ILogger<PersistenceJob> log,
        IConfiguration configuration,
        IDatabaseManagement database,
        IPersistenceProvider persistenceProvider)
    {
        _log = log;
        _database = database;
        _persistenceProvider = persistenceProvider;
        _configuration = configuration.PersistenceJob;
        database.DatabaseUpdates += DatabaseUpdated;
    }

    public async Task Start()
    {
        TimeSpan delay = _configuration.Interval;
        _log.LogInformation("Starting persistence job, checking for write every {Delay}", delay);
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
        _log.LogDebug("Database updated, setting dirty flag");
        _dirty = true;
    }
}
