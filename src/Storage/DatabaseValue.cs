namespace Lesniak.Redis.Storage;

class DatabaseValue
{
    public byte[]? Value { get; }

    public DateTime? Expiration { get; } = null;

    public DatabaseValue(byte[]? value, DateTime? expiration = null)
    {
        Value = value;
        Expiration = expiration;
    }
}