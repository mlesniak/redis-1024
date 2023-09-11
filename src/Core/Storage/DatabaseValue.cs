namespace Lesniak.Redis.Core.Storage;

internal class DatabaseValue
{
    public DatabaseValue(byte[]? value, DateTime? expiration = null)
    {
        Value = value;
        Expiration = expiration;
    }

    public byte[]? Value { get; }

    public DateTime? Expiration { get; }

    public bool Expired(DateTime now)
    {
        if (Expiration < now) return true;
        return false;
    }
}