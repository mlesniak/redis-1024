using Lesniak.Redis;
using Lesniak.Redis.Core;
using Lesniak.Redis.Server;
using Lesniak.Redis.Storage;

var configuration = Configuration.Load();
var storage = new Memory(new DefaultDateTimeProvider());
var commandHandler = new CommandHandler(storage);
var networkServer = new RedisServer(configuration, commandHandler);

networkServer.Run();