namespace Lesniak.Redis.Storage;

class MemoryValue
{
    readonly IDateTimeProvider _dateTimeProvider;

    readonly DateTime? _expiration = null;

    public bool Expired
    {
        get => _expiration != null && _dateTimeProvider.Now > _expiration;
    }

    private byte[]? _value;

    public byte[]? Value
    {
        get
        {
            if (_expiration == null || _dateTimeProvider.Now < _expiration)
            {
                return _value;
            }

            return null;
        }
        set { _value = value; }
    }

    public MemoryValue(IDateTimeProvider dateTimeProvider, byte[]? value, int? expiration = null)
    {
        _dateTimeProvider = dateTimeProvider;
        Value = value;
        if (expiration != null)
        {
            _expiration = _dateTimeProvider.Now.AddMilliseconds((double)expiration);
        }
    }
}
