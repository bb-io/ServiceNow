using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

public class KnowledgeBaseItemDto
{
    [JsonProperty("sys_id")] public string? SysId { get; set; }
    [JsonProperty("title")] public string? Title { get; set; }
}
