namespace Lesniak.Redis.Utils;

public interface IClock
{
    DateTime Now { get; }
}