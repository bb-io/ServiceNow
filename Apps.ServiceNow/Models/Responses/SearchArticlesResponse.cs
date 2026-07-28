using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Utils;
using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Responses;

public class ArticleSearchItem
{
    public ArticleSearchItem() { }

    public ArticleSearchItem(ArticleDto dto)
    {
        ArticleId = dto.SysId;
        Number = dto.Number;
        Title = dto.ShortDescription ?? string.Empty;
        Snippet = HtmlPreview.Build(dto.Text);
        Language = dto.Language ?? string.Empty;
        State = dto.WorkflowState ?? string.Empty;
        KnowledgeBaseId = dto.KnowledgeBase?.Value ?? string.Empty;
        CreatedOn = ServiceNowDate.Parse(dto.CreatedOn);
        UpdatedOn = ServiceNowDate.Parse(dto.UpdatedOn);
    }

    [Display("Article ID")] public string ArticleId { get; set; } = string.Empty;
    [Display("Number")] public string Number { get; set; } = string.Empty;
    [Display("Title")] public string Title { get; set; } = string.Empty;
    [Display("Snippet", Description = "A short plain-text preview of the article body.")] public string Snippet { get; set; } = string.Empty;
    [Display("Language")] public string Language { get; set; } = string.Empty;
    [Display("State", Description = "The workflow state, for example published or draft.")] public string State { get; set; } = string.Empty;
    [Display("Knowledge base ID")] public string KnowledgeBaseId { get; set; } = string.Empty;
    [Display("Created at", Description = "When the article was created (UTC).")] public DateTime? CreatedOn { get; set; }
    [Display("Updated at", Description = "When the article was last updated (UTC).")] public DateTime? UpdatedOn { get; set; }
}

public class SearchArticlesResponse
{
    [Display("Articles")] public List<ArticleSearchItem> Articles { get; set; } = new();
    [Display("Total count", Description = "The number of articles returned.")] public int TotalCount { get; set; }
}
