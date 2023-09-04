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
// TODO(mlesniak) Cleanup job for expired values.
// TODO(mlesniak) ADd tests
public class Memory
{
    private static readonly ILogger _logger = Logging.For<Memory>();

    private readonly IDateTimeProvider _dateTimeProvider;

    private readonly ConcurrentDictionary<string, MemoryValue> _memory = new();

    public Memory(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public void Set(string key, byte[] value, int? expMs)
    {
        _logger.LogInformation($"Storing {key} with expiration {expMs}");
        _memory[key] = new(_dateTimeProvider, value, expMs);
    }

    public byte[]? Get(string key) => _memory.TryGetValue(key, out MemoryValue? value) ? value.Value : null;
}