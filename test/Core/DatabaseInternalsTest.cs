using Lesniak.Redis.Core;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Core;

class TimeoutWrapper
{
    public static async Task ExecuteWithTimeout(TimeSpan timeout, Action action)
    {
        var cts = new CancellationTokenSource(timeout);
        Task task = Task.Run(action, cts.Token);
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // Ignored.
        }
    }
}

public class DatabaseInternalsTest
{
    private readonly IConfiguration _configuration = new TestConfiguration();
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly IDatabaseManagement _sut;

    public DatabaseInternalsTest()
    {
        _dateTimeProvider = new TestDateTimeProvider();
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _dateTimeProvider);
    }

    [Fact]
    public void WriteLock_LocksMultipleWrites_WhileAllowingReads()
    {
        _sut.Set("key", new byte[]
        {
            1
        });

        // Perform some long-running operation which wants to
        // prevent write operations to the database.
        Task.Run(() =>
        {
            _sut.WriteLock(() =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            });
        });

        // Reading values is not blocked.
        var result = _sut.Get("key");

        // Writing values is blocked.
        var successfulWrite = false;
        Task.Run(() => TimeoutWrapper.ExecuteWithTimeout(TimeSpan.FromMilliseconds(50), () =>
        {
            _sut.Set("key", new byte[]
            {
                1,
                2,
                3
            });
            successfulWrite = true;
        }));

        // Wait until all long-running operations have passed and all timeouts have been reached.
        Thread.Sleep(100);

        // Since we were blocked, value has not been written before the timeout.
        False(successfulWrite);
        Equal(new byte[]
        {
            1
        }, result);
    }
}