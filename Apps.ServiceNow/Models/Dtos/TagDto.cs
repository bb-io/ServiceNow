using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

public class TagDto
{
    [JsonProperty("sys_id")]
    public string SysId { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("short_description")]
    public string? ShortDescription { get; set; }

    [JsonProperty("active")]
    public bool Active { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("color")]
    public string? Color { get; set; }

    [JsonProperty("sys_created_on")]
    public DateTime CreatedOn { get; set; }

    [JsonProperty("sys_created_by")]
    public string CreatedBy { get; set; } = string.Empty;

    [JsonProperty("sys_updated_on")]
    public DateTime UpdatedOn { get; set; }

    [JsonProperty("sys_updated_by")]
    public string UpdatedBy { get; set; } = string.Empty;
}