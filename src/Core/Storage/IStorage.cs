namespace Lesniak.Redis.Core.Storage;

public interface IStorage
{
    void Load();
    void Save();
}