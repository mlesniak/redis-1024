using Lesniak.Redis;
using Lesniak.Redis.Core;
using Lesniak.Redis.Core.Storage;
using Lesniak.Redis.Server;

var configuration = Configuration.Load();
var storage = new InMemoryStorage(new DefaultDateTimeProvider());
var commandHandler = new CommandHandler(storage);
var networkServer = new RedisServer(configuration, commandHandler);

networkServer.Run();