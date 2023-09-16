using System.Collections.Concurrent;
using System.Text.Json;

using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Jobs;

public class PersistenceJob : IJob
{
    private readonly IPersistenceProvider _persistenceProvider;
    private static readonly ILogger log = Logging.For<PersistenceJob>();
    private bool _dirty;

    public PersistenceJob(IDatabaseManagement database, IPersistenceProvider persistenceProvider)
    {
        _persistenceProvider = persistenceProvider;
        database.DatabaseUpdates += DatabaseUpdated;
    }

    private void DatabaseUpdated()
    {
        log.LogDebug("Database updated, setting dirty flag");
        _dirty = true;
    }

    public async Task Start()
    {
        // TODO(mlesniak) Configurable
        var delay = TimeSpan.FromSeconds(5);
        log.LogInformation("Starting persistence job, checking for write every {Delay}", delay);
        while (true)
        {
            if (_dirty)
            {
                _persistenceProvider.Save();
                _dirty = false;
            }

            await Task.Delay(delay);
        }
    }
}