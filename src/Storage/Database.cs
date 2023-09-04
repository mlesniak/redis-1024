using System.Collections.Concurrent;

using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Storage;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime Now
    {
        get => DateTime.Now;
    }
}

class MemoryValue
{
    readonly IDateTimeProvider _dateTimeProvider;

    readonly DateTime? _expiration = null;

    public bool Expired
    {
        get => _expiration != null && _dateTimeProvider.Now > _expiration;
    }

    private byte[]? _value;

    public byte[]? Value
    {
        get
        {
            if (_expiration == null || _dateTimeProvider.Now < _expiration)
            {
                return _value;
            }

            return null;
        }
        set { _value = value; }
    }

    public MemoryValue(IDateTimeProvider dateTimeProvider, byte[]? value, int? expiration = null)
    {
        _dateTimeProvider = dateTimeProvider;
        Value = value;
        if (expiration != null)
        {
            _expiration = _dateTimeProvider.Now.AddMilliseconds((double)expiration);
        }
    }
}

// TODO(mlesniak) plain persistence
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

    private async Task StartBackgroundCleanup()
    {
        while (true)
        {
            // Run once a minute.
            await Task.Delay(1_000 * 5);

            var removed = 0;
            foreach (KeyValuePair<string,MemoryValue> pair in _memory)
            {
                if (!pair.Value.Expired)
                {
                    continue;
                }

                _memory.Remove(pair.Key, out _);
                removed++;
            }
            
            _logger.LogInformation("Cleaned up. Removed {Removed} entries", removed);
        }
    }

    public void Set(string key, byte[] value, int? expMs)
    {
        _logger.LogInformation("Storing {Key} with expiration {Expiration}", key, expMs);
        _memory[key] = new(_dateTimeProvider, value, expMs);
    }

    public byte[]? Get(string key) => _memory.TryGetValue(key, out MemoryValue? value) ? value.Value : null;
}