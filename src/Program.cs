using Lesniak.Redis;
using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

var log = Logging.For<Program>();
var configuration = Configuration.Get();
log.LogInformation($"Configuration {configuration}");