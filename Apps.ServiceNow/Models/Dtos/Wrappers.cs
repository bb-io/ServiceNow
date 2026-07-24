using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

public class ResultWrapper<T>
{
    [JsonProperty("result")] public T Result { get; set; } = default!;
}

public class ResultListWrapper<T>
{
    [JsonProperty("result")] public List<T> Result { get; set; } = new();
}

public class ReferenceValueDto
{
    [JsonProperty("link")] public string? Link { get; set; }
    [JsonProperty("value")] public string? Value { get; set; }
}
