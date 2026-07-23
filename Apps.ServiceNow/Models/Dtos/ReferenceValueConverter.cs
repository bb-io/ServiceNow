using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.ServiceNow.Models.Dtos;

/// <summary>
/// ServiceNow returns reference fields either as an object {"link","value"} (raw mode) or as a
/// plain string — the display label with sysparm_display_value, or "" when the field is empty.
/// This converter accepts both shapes so a reference DTO deserializes reliably.
/// </summary>
public class ReferenceValueConverter : JsonConverter<ReferenceValueDto?>
{
    public override ReferenceValueDto? ReadJson(JsonReader reader, Type objectType,
        ReferenceValueDto? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);

        if (token.Type == JTokenType.Object)
            return token.ToObject<ReferenceValueDto>();

        if (token.Type is JTokenType.String or JTokenType.Null)
        {
            var value = token.Value<string>();
            return string.IsNullOrEmpty(value) ? null : new ReferenceValueDto { Value = value };
        }

        return null;
    }

    public override void WriteJson(JsonWriter writer, ReferenceValueDto? value, JsonSerializer serializer)
        => serializer.Serialize(writer, value);
}
