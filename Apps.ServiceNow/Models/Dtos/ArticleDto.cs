using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

/// <summary>A kb_knowledge record as returned by the Table API.</summary>
public class ArticleDto
{
    [JsonProperty("sys_id")] public string SysId { get; set; } = string.Empty;
    [JsonProperty("number")] public string Number { get; set; } = string.Empty;
    [JsonProperty("short_description")] public string? ShortDescription { get; set; }
    [JsonProperty("workflow_state")] public string? WorkflowState { get; set; }
    [JsonProperty("language")] public string? Language { get; set; }
    [JsonProperty("article_type")] public string? ArticleType { get; set; }
    [JsonProperty("text")] public string? Text { get; set; }
    [JsonProperty("sys_created_on")] public string? CreatedOn { get; set; }
    [JsonProperty("sys_updated_on")] public string? UpdatedOn { get; set; }

    [JsonProperty("kb_knowledge_base")]
    [JsonConverter(typeof(ReferenceValueConverter))]
    public ReferenceValueDto? KnowledgeBase { get; set; }

    [JsonProperty("kb_category")]
    [JsonConverter(typeof(ReferenceValueConverter))]
    public ReferenceValueDto? Category { get; set; }

    [JsonProperty("author")]
    [JsonConverter(typeof(ReferenceValueConverter))]
    public ReferenceValueDto? Author { get; set; }
}
