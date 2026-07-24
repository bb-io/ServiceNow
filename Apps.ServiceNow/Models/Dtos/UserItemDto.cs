using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

public class UserItemDto
{
    [JsonProperty("sys_id")] public string SysId { get; set; } = string.Empty;
    [JsonProperty("name")] public string? Name { get; set; }
    [JsonProperty("user_name")] public string? UserName { get; set; }
}
