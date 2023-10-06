namespace Lesniak.Redis.Utils;

public class Clock : IClock
{
    public DateTime Now => DateTime.UtcNow;
}