namespace Lesniak.Redis.Core;

public interface IDatabase
{
    void Set(string key, byte[] value, TimeSpan? expiration = null);
    byte[]? Get(string key);

    /// <summary>
    /// Removes a single key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>true, if the key was removed, false if not, e.g. the key does not exist.</returns>
    bool Remove(string key);

    int Publish(string channel, byte[] message);
    int Subscribe(string clientId, string channel, Database.AsyncMessageReceiver receiver);
    void Unsubscribe(string clientId, string channel);
    IEnumerable<string> UnsubscribeAll(string clientId);

    bool VerifyPassword(string password);
    bool AuthenticationRequired { get; }
}
