using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos.Label;

public class LabelEntryDto
{
    [JsonProperty("sys_id")]
    public string SysId { get; set; } = string.Empty;
    
    [JsonProperty("table_key")]
    public string? TableKey { get; set; }

    [JsonProperty("label")]
    public string? Label { get; set; }
}