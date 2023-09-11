using System.Collections;

namespace Lesniak.Redis.Core.Storage;

public interface IDatabase : IEnumerable
{
    public delegate void DataChangedHandler();

    /// <summary>
    ///     Returns number of stored items.
    ///     Note that we return even expired items, which have
    ///     not been cleaned up yet.
    /// </summary>
    /// <returns>Number of keys in the memory structure</returns>
    int Count { get; }

    void Set(string key, byte[] value, int? expirationMs);

    byte[]? Get(string key);

    void Remove(string key);
}