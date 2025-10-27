using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrevisionalAccountManager.JsonConverters;

public class BooleanJsonConverter : JsonConverter<bool>
{

    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if ( reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False )
        {
            return reader.GetBoolean();
        }
        if ( reader.TokenType == JsonTokenType.Number )
        {
            return reader.GetInt32() != 0;
        }
        if ( reader.TokenType == JsonTokenType.String )
        {
            var stringValue = reader.GetString();
            bool.TryParse(stringValue, out var boolValue);
            return boolValue;
        }

        throw new JsonException($"could not parse boolean: {reader.GetString()}");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}