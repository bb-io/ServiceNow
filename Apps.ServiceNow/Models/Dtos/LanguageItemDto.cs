using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

public class LanguageItemDto
{
    [JsonProperty("id")] public string? Id { get; set; }
    [JsonProperty("name")] public string? Name { get; set; }
}
