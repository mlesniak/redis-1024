using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lesniak.Redis.Core.Storage;

internal class DatabaseValueConverter : JsonConverter<DatabaseValue>
{
    public override DatabaseValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;
        byte[] value = root.GetProperty("value").GetBytesFromBase64();
        DateTime? expiration =
            root.TryGetProperty("expiration", out JsonElement expElement)
                ? expElement.GetDateTime()
                : null;
        return new DatabaseValue(value, expiration);
    }

    public override void Write(Utf8JsonWriter writer, DatabaseValue value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBase64String("value", value.Value);
        if (value.Expiration != null)
        {
            writer.WriteString("expiration", value.Expiration?.ToString("O"));
        }

        writer.WriteEndObject();
    }
}