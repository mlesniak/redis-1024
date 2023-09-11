namespace Lesniak.Redis.Core.Storage.Jobs;

public interface IDatabaseJob
{
    void Run(Configuration configuration);
}