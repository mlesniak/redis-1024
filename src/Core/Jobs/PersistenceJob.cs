using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Core.Jobs;

public class PersistenceJob : IJob
{
    private static readonly ILogger log = Logging.For<PersistenceJob>();

    public Task Start()
    {
        var delay = TimeSpan.FromMinutes(1);
        log.LogInformation($"Starting persistence job, every {delay}");
        return Task.CompletedTask;
    }
}