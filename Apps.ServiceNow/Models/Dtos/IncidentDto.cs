using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

/// <summary>An incident record as returned by the Table API (raw values).</summary>
public class IncidentDto
{
    [JsonProperty("sys_id")] public string SysId { get; set; } = string.Empty;
    [JsonProperty("number")] public string Number { get; set; } = string.Empty;
    [JsonProperty("short_description")] public string? ShortDescription { get; set; }
    [JsonProperty("description")] public string? Description { get; set; }
    [JsonProperty("state")] public string? State { get; set; }
    [JsonProperty("priority")] public string? Priority { get; set; }
    [JsonProperty("urgency")] public string? Urgency { get; set; }
    [JsonProperty("impact")] public string? Impact { get; set; }
    [JsonProperty("sys_created_on")] public string? CreatedOn { get; set; }
    [JsonProperty("sys_updated_on")] public string? UpdatedOn { get; set; }

    [JsonProperty("caller_id")]
    [JsonConverter(typeof(ReferenceValueConverter))]
    public ReferenceValueDto? CallerId { get; set; }

    [JsonProperty("assigned_to")]
    [JsonConverter(typeof(ReferenceValueConverter))]
    public ReferenceValueDto? AssignedTo { get; set; }
}
