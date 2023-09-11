using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Storage.Jobs;

public class PersistenceJob : IDatabaseJob
{
    private static readonly ILogger _logger = Logging.For<MemoryCleanupJob>();
    private readonly IStorage _storage;
    private bool _dirty;

    public PersistenceJob(IStorage storage)
    {
        _storage = storage;
    }

    public void Run(Configuration configuration)
    {
        Task.Run(Run);
    }

    async Task Run()
    {
        _logger.LogInformation("Spawning persistence job");
        while (true)
        {
            if (_dirty)
            {
                _dirty = false;
                // TODO(mlesniak) Where does locking happen to
                // prevent multiple writes?
                _storage.Save();
            }

            // TODO(mlesniak) Configurable
            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }

    public void DataChangedHandler()
    {
        _dirty = true;
    }
}