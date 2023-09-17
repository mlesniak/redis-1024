using System.Diagnostics;

using StackExchange.Redis;

TextWriter traceWriter = new StringWriter();
Trace.Listeners.Add(new TextWriterTraceListener(traceWriter));
var options = new ConfigurationOptions { EndPoints = { "localhost" } };
options.SyncTimeout = 5000;

try
{
    ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options, traceWriter);
    IDatabase db = redis.GetDatabase();

    db.StringSet("michael", "lesniak");
    string? value = db.StringGet("michael");
    Console.WriteLine($"Value: {value}");
}
catch (Exception e)
{
    Console.WriteLine($"Exception: {e.Message}");
    Console.WriteLine(traceWriter.ToString());
}
// redis.PreserveAsyncOrder = false;
// redis.InternalError += (sender, args) => Console.WriteLine(args.Exception.Message);
// redis.ConnectionFailed += (sender, args) => Console.WriteLine(args.Exception.Message);
// redis.ConfigurationChanged += (sender, args) => Console.WriteLine(args);