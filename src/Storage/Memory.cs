using System.Collections.Concurrent;

using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Storage;

// TODO(mlesniak) plain persistence
public class Memory
{
    private static readonly ILogger _logger = Logging.For<Memory>();

    // We will add expiration time later.
    private readonly ConcurrentDictionary<string, byte[]> _memory = new();

    // TODO(mlesniak) expiration time
    public void Set(string key, byte[] value, int? expMs)
    {
        _logger.LogInformation($"Storing {key} with expiration {expMs}");
        _memory[key] = value;
    }

    public byte[]? Get(string key) => _memory.TryGetValue(key, out byte[]? value) ? value : null;
}