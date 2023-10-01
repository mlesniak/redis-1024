using System.Diagnostics;
using System.Globalization;

using StackExchange.Redis;

// Enable debug logging in case we have an error.
TextWriter traceWriter = new StringWriter();
Trace.Listeners.Add(new TextWriterTraceListener(traceWriter));

try
{
    ConfigurationOptions options = new() { EndPoints = { "localhost" }, SyncTimeout = 5000, Password = "foo"};
    ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options, traceWriter);

    // Store and retrieve a key.
    IDatabase db = redis.GetDatabase();
    string key = "current";
    db.StringSet(key, DateTime.Now.ToString(CultureInfo.CurrentCulture));
    string? value = db.StringGet(key);
    Console.WriteLine($"Value: {value}");

    // Publish and subscribe.
    ISubscriber sub = redis.GetSubscriber();
    sub.Subscribe("receiving", (channel, message) =>
    {
        Console.WriteLine($"Received message: {message} on channel: {channel}");
    });
    while (true)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        sub.Publish("date", DateTime.Now.ToString());
    }
}
catch (Exception e)
{
    Console.WriteLine($"Exception: {e.Message}");
    Console.WriteLine(traceWriter.ToString());
    Environment.Exit(1);
}