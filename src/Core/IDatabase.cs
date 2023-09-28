namespace Lesniak.Redis.Core;

public interface IDatabase
{
    void Set(string key, byte[] value, long? expiration = null);
    byte[]? Get(string key);
    void Remove(string key);

    int Publish(string channel, byte[] message);
    int Subscribe(string clientId, string channel, Database.AsyncMessageReceiver receiver);
    void Unsubscribe(string clientId, string channel);
    IEnumerable<string> UnsubscribeAll(string clientId);
    bool VerifyPassword(string password);
}