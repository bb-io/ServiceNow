using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.ServiceNow.Utils.JsonConverters;

public class ReferenceConverter : JsonConverter<string?>
{
    public override string? ReadJson(
        JsonReader reader,
        Type objectType,
        string? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        return reader.TokenType switch
        {
            JsonToken.Null => null,
            JsonToken.String => (string?)reader.Value,
            JsonToken.StartObject => JObject.Load(reader)["value"]?.ToString(),
            _ => null,
        };
    }

    public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }
}