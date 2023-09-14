using Lesniak.Redis.Core;
using Lesniak.Redis.Core.Jobs;
using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// var configuration = Configuration.Get();
// // log.LogInformation($"Configuration {configuration}");
//
// DefaultDateTimeProvider dateTimeProvider = new();
// var database = new Database(dateTimeProvider);
// database.Set("michael", "foo"u8.ToArray(), 1000);
// await Task.Delay(200);
//
// CleanupJob cleanupJob = new (dateTimeProvider, database);
// cleanupJob.Run();
//
// var c = database.Count;
// log.LogInformation("Count = {Count}", c);

// var bytes = database.Get("michael");
// string? str = null;
// if (bytes != null)
// {
//     str = Encoding.ASCII.GetString(bytes);
// }
//
// log.LogInformation($"Value {str}");

// database.Remove("michael");
// bytes = database.Get("michael");
// if (bytes == null)
// {
//     log.LogInformation("null");
// }

var log = Logging.For<Program>();
var serviceProvider = new ServiceCollection()
    .AddSingleton<IDateTimeProvider>(new DefaultDateTimeProvider())
    .AddSingleton<Database>()
    .AddSingleton<CleanupJob>()
    .BuildServiceProvider();

var database = serviceProvider.GetRequiredService<Database>();
database.Set("michael", "foo"u8.ToArray(), 1000);
await Task.Delay(200);

CleanupJob cleanupJob = serviceProvider.GetRequiredService<CleanupJob>();
cleanupJob.Run();

var c = database.Count;
log.LogInformation("Count = {Count}", c);

