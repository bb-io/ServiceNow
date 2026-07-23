using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

/// <summary>A single article entry inside a KM search response.</summary>
public class KmSearchArticleDto
{
    [JsonProperty("id")] public string? Id { get; set; }
    [JsonProperty("number")] public string? Number { get; set; }
    [JsonProperty("title")] public string? Title { get; set; }
    [JsonProperty("snippet")] public string? Snippet { get; set; }
    [JsonProperty("score")] public double Score { get; set; }
}

/// <summary>The result block of a KM search response: {"result":{"meta":{...},"articles":[...]}}</summary>
public class KmSearchResultDto
{
    [JsonProperty("meta")] public KmSearchMetaDto? Meta { get; set; }
    [JsonProperty("articles")] public List<KmSearchArticleDto> Articles { get; set; } = new();
    [JsonProperty("error_msg")] public string? ErrorMsg { get; set; }
}

public class KmSearchMetaDto
{
    [JsonProperty("count")] public double Count { get; set; }
    [JsonProperty("status")] public KmStatusDto? Status { get; set; }
}

public class KmStatusDto
{
    [JsonProperty("code")] public double Code { get; set; }
}
