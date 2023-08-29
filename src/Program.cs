using Lesniak.Redis.Core;
using Lesniak.Redis.Server;
using Lesniak.Redis.Storage;

var storage = new Memory();
var commandHandler = new CommandHandler(storage);
var networkServer = new RedisServer(commandHandler);

networkServer.Run();
