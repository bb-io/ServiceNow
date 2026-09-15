using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos.Label;

public class LabelDto
{
    [JsonProperty("value")]
    public string Value { get; set; } = string.Empty;
}