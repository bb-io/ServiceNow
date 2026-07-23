using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

/// <summary>Table / KM API single-object envelope: {"result": { ... }}</summary>
public class ResultWrapper<T>
{
    [JsonProperty("result")] public T Result { get; set; } = default!;
}

/// <summary>Table API list envelope: {"result": [ ... ]}</summary>
public class ResultListWrapper<T>
{
    [JsonProperty("result")] public List<T> Result { get; set; } = new();
}

/// <summary>A ServiceNow reference field: {"link": "...", "value": "sys_id"}. Deserializes from an object.</summary>
public class ReferenceValueDto
{
    [JsonProperty("link")] public string? Link { get; set; }
    [JsonProperty("value")] public string? Value { get; set; }
}
