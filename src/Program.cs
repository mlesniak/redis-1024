using Lesniak.Redis.Core;
using Lesniak.Redis.Core.Jobs;
using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lesniak.Redis;

class Program
{
    private static ILogger log = Logging.For<Program>();
    private ServiceProvider _serviceProvider;

    void AddServices()
    {
        _serviceProvider = new ServiceCollection()
            .AddSingleton<IDateTimeProvider>(new DefaultDateTimeProvider())
            .AddSingleton<Database>()
            .AddSingleton<IJob, CleanupJob>()
            .AddSingleton<IJob, PersistenceJob>()
            .BuildServiceProvider();
    }

    void SpawnJobs()
    {
        foreach (var job in _serviceProvider.GetServices<IJob>())
        {
            _ = Task.Run(() => job.Start());
        }
    }

    async Task Test()
    {
        var database = _serviceProvider.GetRequiredService<Database>();
        database.Set("michael", "foo"u8.ToArray(), 100);
        await Task.Delay(100);
        var c = database.Count;
        log.LogInformation("Count = {Count}", c);
    }

    public static async Task Main()
    {
        var program = new Program();
        program.AddServices();
        program.SpawnJobs();
        await program.Test();
    }
}