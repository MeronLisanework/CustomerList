using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomerList.Models;

// Mirrors LenientStringConverter's reasoning but for nullable ints:
// accepts a JSON number normally, but also tolerates a numeric string
// (e.g. "12" instead of 12) instead of throwing.
public class LenientNullableIntConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.Number:
                return reader.GetInt32();
            case JsonTokenType.String:
                var text = reader.GetString();
                return int.TryParse(text, out var parsed) ? parsed : null;
            default:
                reader.Skip();
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
