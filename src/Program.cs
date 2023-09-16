using System.Text;

using Lesniak.Redis.Core;
using Lesniak.Redis.Core.Jobs;
using Lesniak.Redis.Core.Persistence;
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
            .AddSingleton<IDatabase, Database>()
            .AddSingleton<IDatabaseManagement>(sp => (IDatabaseManagement)sp.GetRequiredService<IDatabase>())
            .AddSingleton<IJob, CleanupJob>()
            .AddSingleton<IJob, PersistenceJob>()
            .AddSingleton<IPersistenceProvider, JsonPersistence>()
            .BuildServiceProvider();
    }

    void SpawnJobs()
    {
        foreach (var job in _serviceProvider.GetServices<IJob>())
        {
            _ = Task.Run(() => job.Start());
        }
    }

    private void LoadDatabase()
    {
        var provider = _serviceProvider.GetRequiredService<IPersistenceProvider>();
        provider.Load();
    }

    public static async Task Main()
    {
        var program = new Program();
        program.AddServices();
        program.SpawnJobs();
        program.LoadDatabase();
        await program.Test();
    }

    async Task Test()
    {
        var database = _serviceProvider.GetRequiredService<IDatabase>();
        var databaseManagement = _serviceProvider.GetRequiredService<IDatabaseManagement>();
        var value = database.Get("michael");
        Console.WriteLine("key = {0}", Encoding.ASCII.GetString(value));
        // database.Set("michael", "foo"u8.ToArray());
        await Task.Delay(5000);
    }
}