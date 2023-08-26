namespace Lesniak.Redis;

public class Memory
{
    // We will add expiration time later.
    protected readonly Dictionary<string, byte[]> _memory = new();

    public void Set(string key, byte[] value)
    {
        _memory[key] = value;
    }

    public byte[]? Get(string key) => _memory.TryGetValue(key, out byte[]? value) ? value : null;
}