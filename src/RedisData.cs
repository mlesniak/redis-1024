namespace Lesniak.Redis;

public class RedisData
{
    public RedisDataType Type { get; set; }
    public string? BulkString { get; set; }
    public List<RedisData>? ArrayValues { get; set; }
    
    public override string ToString()
    {
        return "TODO";
    }
}