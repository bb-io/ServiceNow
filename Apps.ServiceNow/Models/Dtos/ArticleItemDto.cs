using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

public class ArticleItemDto
{
    [JsonProperty("sys_id")] public string? SysId { get; set; }
    [JsonProperty("number")] public string? Number { get; set; }
    [JsonProperty("short_description")] public string? ShortDescription { get; set; }
}
