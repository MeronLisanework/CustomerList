using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomerList.Models;

// The API is inconsistent: some fields typed as strings in most records
// (e.g. "title") occasionally come back as a raw JSON number instead.
// This converter accepts string, number, or boolean tokens and always
// produces a string, instead of System.Text.Json's default strict
// behaviour which throws on a type mismatch.
public class LenientStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                // Utf8JsonReader has no GetRawText() (that's on JsonElement).
                // Decode the raw UTF-8 bytes of the number token directly,
                // which avoids precision loss and works for ints or decimals.
                return System.Text.Encoding.UTF8.GetString(reader.ValueSpan);
            case JsonTokenType.True:
                return "true";
            case JsonTokenType.False:
                return "false";
            default:
                // Fallback: skip and return null rather than throwing,
                // so one unexpected token never breaks the whole list.
                reader.Skip();
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}
