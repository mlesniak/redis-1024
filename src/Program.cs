using System.Text;

using Lesniak.Redis;
using Lesniak.Redis.Utils;
using Lesniak.Redis.Core;

using Microsoft.Extensions.Logging;

var log = Logging.For<Program>();
var configuration = Configuration.Get();
log.LogInformation($"Configuration {configuration}");

var database = new Database();
database.Set("michael", "foo"u8.ToArray());
var bytes = database.Get("michael");
var str = Encoding.ASCII.GetString(bytes);
Console.WriteLine("michael = {0}", str);
database.Remove("michael");
bytes = database.Get("michael");
if (bytes == null)
{
    Console.WriteLine("null");
}