using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lesniak.Redis.Storage;

class DatabaseValueConverter : JsonConverter<DatabaseValue>
{
    public override DatabaseValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, DatabaseValue value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBase64String("value", value.Value);
        if (value.Expiration != null)
        {
            // We ignore any timezone information for now.
            writer.WriteString("expiration", value.Expiration?.ToString("O"));
        }
        writer.WriteEndObject();
    }
}

class DatabaseValue
{
    readonly IDateTimeProvider _dateTimeProvider;

    private readonly DateTime? _expiration = null;

    public bool Expired
    {
        get => _expiration != null && _dateTimeProvider.Now > _expiration;
    }

    private readonly byte[]? _value;

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
        private init { _value = value; }
    }

    public DateTime? Expiration
    {
        get => _expiration;
    }

    public DatabaseValue(IDateTimeProvider dateTimeProvider, byte[]? value, int? expiration = null)
    {
        _dateTimeProvider = dateTimeProvider;
        Value = value;
        if (expiration != null)
        {
            _expiration = _dateTimeProvider.Now.AddMilliseconds((double)expiration);
        }
    }
}