using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.ServiceNow.Models.Dtos;

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
