﻿using Lesniak.Redis.Communication.Network;
using Lesniak.Redis.Core;
using Lesniak.Redis.Core.Jobs;
using Lesniak.Redis.Core.Persistence;
using Lesniak.Redis.Utils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lesniak.Redis;

// TODO(mlesniak) Documentation
class Program
{
    private ServiceProvider _serviceProvider;

    private void AddServices()
    {
        _serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.TimestampFormat = "HH:mm:ss ";
                });
            })
            .AddSingleton<IConfiguration, Configuration>()
            .AddSingleton<IClock>(new Clock())
            .AddSingleton<IDatabase, Database>()
            .AddSingleton<IDatabaseManagement>(sp => (IDatabaseManagement)sp.GetRequiredService<IDatabase>())
            .AddSingleton<IJob, CleanupJob>()
            .AddSingleton<IJob, PersistenceJob>()
            .AddSingleton<IPersistenceProvider, JsonPersistence>()
            .AddSingleton<ClientHandler>()
            .AddSingleton<NetworkServer>()
            .BuildServiceProvider();
    }

    // I know that there are more idiomatic approaches to this, but for the time being
    // this is good enough and I'll explore other interesting topics first. I might
    // come back to this later, though, let's be honest, this is rather unlikely. ;-)
    private void SpawnJobs()
    {
        foreach (IJob job in _serviceProvider.GetServices<IJob>())
        {
            _ = Task.Run(() => job.Start());
        }
    }

    private void LoadDatabase()
    {
        IPersistenceProvider provider = _serviceProvider.GetRequiredService<IPersistenceProvider>();
        provider.Load();
    }

    public static async Task Main()
    {
        Program program = new Program();
        program.AddServices();
        program.SpawnJobs();
        program.LoadDatabase();
        program.StartNetworkServer();
        // await program.Test();
    }

    private void StartNetworkServer()
    {
        NetworkServer server = _serviceProvider.GetRequiredService<NetworkServer>();
        IDatabase database = _serviceProvider.GetRequiredService<IDatabase>();

        // __Booksleeve_TieBreak is a key used by the BookSleeve Redis
        // client library for .NET. It's used to conduct a tie-break
        // when deciding which master to promote in a Redis master-
        // slave setup. The tie-break decision is based on the time
        // at which the key was set.
        //
        // Libraries check if the key exits on connecting.
        database.Set("__Booksleeve_TieBreak", "OK"u8.ToArray());

        server.Start();
    }
}
