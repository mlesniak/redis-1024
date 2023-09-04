using System.Collections.Concurrent;

using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Storage;

public class Database
{
    private static readonly ILogger _logger = Logging.For<Database>();

    private readonly IDateTimeProvider _dateTimeProvider;

    private readonly ConcurrentDictionary<string, MemoryValue> _memory = new();

    public Database(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        Task.Run(StartBackgroundCleanup);
    }

    // TODO(mlesniak) Add tests...
    private async Task StartBackgroundCleanup()
    {
        while (true)
        {
            // Run once a minute.
            await Task.Delay(1_000 * 5);

            var removed = 0;
            foreach (KeyValuePair<string, MemoryValue> pair in _memory)
            {
                if (!pair.Value.Expired)
                {
                    continue;
                }

                _memory.Remove(pair.Key, out _);
                removed++;
            }

            _logger.LogDebug("Cleaned up. Removed {Removed} entries", removed);
        }
    }

    public void Set(string key, byte[] value, int? expMs)
    {
        _logger.LogInformation("Storing {Key} with expiration {Expiration}", key, expMs);
        _memory[key] = new(_dateTimeProvider, value, expMs);
    }

    public byte[]? Get(string key) => _memory.TryGetValue(key, out MemoryValue? value) ? value.Value : null;
}