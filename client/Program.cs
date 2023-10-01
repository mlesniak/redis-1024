using System.Diagnostics;

using StackExchange.Redis;

// Enable debug logging in case we have an error.
TextWriter traceWriter = new StringWriter();
Trace.Listeners.Add(new TextWriterTraceListener(traceWriter));

try
{
    ConfigurationOptions options = new ConfigurationOptions { EndPoints = { "localhost" }, SyncTimeout = 5000 };
    ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options, traceWriter);

    // IDatabase db = redis.GetDatabase();
    // string key = "current";
    // db.StringSet(key, DateTime.Now.ToString(CultureInfo.CurrentCulture));
    // string? value = db.StringGet(key);
    // Console.WriteLine($"Value: {value}");

    ISubscriber sub = redis.GetSubscriber();
    sub.Subscribe("foo", (channel, message) =>
    {
        Console.WriteLine($"Received message: {message} on channel: {channel}");
    });
    while (true)
    {
        await Task.Delay(TimeSpan.FromSeconds(10));
        sub.Publish("date", DateTime.Now.ToString());
    }
}
catch (Exception e)
{
    Console.WriteLine($"Exception: {e.Message}");
    Console.WriteLine(traceWriter.ToString());
    throw;
}