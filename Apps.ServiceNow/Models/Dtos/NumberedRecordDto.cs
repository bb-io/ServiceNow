using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

/// <summary>The identifying pair shared by every numbered ServiceNow record, for labelling references.</summary>
public class NumberedRecordDto
{
    [JsonProperty("sys_id")] public string? SysId { get; set; }
    [JsonProperty("number")] public string? Number { get; set; }
}
