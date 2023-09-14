namespace Lesniak.Redis.Core.Jobs;

public interface IJob
{
    Task Start();
}