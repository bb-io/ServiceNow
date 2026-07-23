using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

/// <summary>A sys_journal_field entry (a single incident comment or work note).</summary>
public class JournalEntryDto
{
    [JsonProperty("sys_id")] public string SysId { get; set; } = string.Empty;
    [JsonProperty("value")] public string? Value { get; set; }
    [JsonProperty("sys_created_by")] public string? CreatedBy { get; set; }
    [JsonProperty("sys_created_on")] public string? CreatedOn { get; set; }
    [JsonProperty("element")] public string? Element { get; set; }
}
