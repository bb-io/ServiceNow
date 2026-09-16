using Apps.ServiceNow.Utils.JsonConverters;
using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos.Label;

public class LabelEntryDto
{
    [JsonProperty("sys_id")]
    public string SysId { get; set; } = string.Empty;
    
    [JsonProperty("table_key")]
    public string? TableKey { get; set; }

    // Can be either a string or an object
    [JsonProperty("label"), JsonConverter(typeof(ReferenceConverter))]
    public string? Label { get; set; }
}