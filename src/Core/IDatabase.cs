namespace Lesniak.Redis.Core;

public interface IDatabase
{
    void Set(string key, byte[] value, long? expiration = null);
    byte[]? Get(string key);
    void Remove(string key);
}