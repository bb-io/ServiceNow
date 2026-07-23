using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

public class KnowledgeBaseItemDto
{
    [JsonProperty("sys_id")] public string? SysId { get; set; }
    [JsonProperty("title")] public string? Title { get; set; }
}

public class LanguageItemDto
{
    [JsonProperty("id")] public string? Id { get; set; }
    [JsonProperty("name")] public string? Name { get; set; }
}

public class UserItemDto
{
    [JsonProperty("sys_id")] public string SysId { get; set; } = string.Empty;
    [JsonProperty("name")] public string? Name { get; set; }
    [JsonProperty("user_name")] public string? UserName { get; set; }
}
