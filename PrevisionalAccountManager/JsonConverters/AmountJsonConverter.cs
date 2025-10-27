using System.Text.Json;
using System.Text.Json.Serialization;
using PrevisionalAccountManager.Models;

namespace PrevisionalAccountManager.JsonConverters;

public class AmountJsonConverter : JsonConverter<Amount>
{
    public override Amount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if ( reader.TokenType == JsonTokenType.Number )
        {
            return reader.GetDouble();
        }

        if ( reader.TokenType == JsonTokenType.String )
        {
            var stringValue = reader.GetString();
            if ( Amount.TryParse(stringValue, null, out var decimalValue) )
            {
                return decimalValue;
            }
        }

        return new Amount();
    }

    public override void Write(Utf8JsonWriter writer, Amount value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}