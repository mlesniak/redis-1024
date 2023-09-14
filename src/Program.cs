using System.Text;

using Lesniak.Redis;
using Lesniak.Redis.Core;
using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

var log = Logging.For<Program>();
var configuration = Configuration.Get();
log.LogInformation($"Configuration {configuration}");

var database = new Database(new DefaultDateTimeProvider());
database.Set("michael", "foo"u8.ToArray(), 1500);
await Task.Delay(1000);
var bytes = database.Get("michael");
string? str = null;
if (bytes != null)
{
    str = Encoding.ASCII.GetString(bytes);
}

log.LogInformation($"Value {str}");

// database.Remove("michael");
// bytes = database.Get("michael");
// if (bytes == null)
// {
//     log.LogInformation("null");
// }