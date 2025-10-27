using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrevisionalAccountManager.JsonConverters;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if ( reader.TokenType == JsonTokenType.String )
        {
            var stringValue = reader.GetString();
            if ( DateTime.TryParse(stringValue, null, out var date) )
            {
                return date;
            }
        }
        throw new JsonException($"could not parse date: {reader.GetString()}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}