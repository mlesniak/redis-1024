namespace Lesniak.Redis;

public class CommandHandler
{
    private readonly Memory _memory;

    public CommandHandler(Memory memory)
    {
        _memory = memory;
    }

    public void Set(string key, byte[] value)
    {
        _memory.Set(key, value);
    }

    public byte[]? Get(string key) => _memory.Get(key);
}