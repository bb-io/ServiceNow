using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos.Label;

public class LabelEntryDto
{
    [JsonProperty("table_key")]
    public string? TableKey { get; set; }

    [JsonProperty("label")]
    public LabelDto? Label { get; set; }
}