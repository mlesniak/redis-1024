using StackExchange.Redis;

ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");
IDatabase db = redis.GetDatabase();

db.StringSet("michael", "lesniak");
string? value = db.StringGet("michael");
Console.WriteLine($"Value: {value}");