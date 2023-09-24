namespace Lesniak.Redis.Core;

public interface IDatabase
{
    void Set(string key, byte[] value, long? expiration = null);
    byte[]? Get(string key);
    void Remove(string key);

    int Publish(string channel, byte[] message);
    void Subscribe(string channel, Database.AsyncMessageReceiver receiver);
    void Unsubscribe(string channel, Database.AsyncMessageReceiver receiver);
}