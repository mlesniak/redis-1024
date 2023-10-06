namespace Lesniak.Redis.Core.Jobs;

public interface IPersistenceProvider
{
    void Save();

    /// <summary>
    ///     Load database from disk. Existing values are removed.
    /// </summary>
    void Load();
}
