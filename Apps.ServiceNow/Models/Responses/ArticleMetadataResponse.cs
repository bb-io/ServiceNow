using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Utils;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Responses;

public class ArticleMetadataResponse : IDownloadContentInput
{
    public ArticleMetadataResponse() { }

    public ArticleMetadataResponse(ArticleDto dto)
    {
        ContentId = dto.SysId;
        Number = dto.Number;
        Title = dto.ShortDescription ?? string.Empty;
        State = dto.WorkflowState ?? string.Empty;
        KnowledgeBaseId = dto.KnowledgeBase?.Value ?? string.Empty;
        CategoryId = dto.Category?.Value ?? string.Empty;
        AuthorId = dto.Author?.Value ?? string.Empty;
        Language = dto.Language ?? string.Empty;
        ArticleType = dto.ArticleType ?? string.Empty;
        Body = dto.Text ?? string.Empty;
        CreatedAt = ServiceNowDate.Parse(dto.CreatedOn);
        UpdatedAt = ServiceNowDate.Parse(dto.UpdatedOn);
    }

    [Display("Article ID")] public string ContentId { get; set; } = string.Empty;
    [Display("Number", Description = "The human-readable article number, for example KB0000024.")] public string Number { get; set; } = string.Empty;
    [Display("Title")] public string Title { get; set; } = string.Empty;
    [Display("State", Description = "The workflow state, for example published or draft.")] public string State { get; set; } = string.Empty;
    [Display("Knowledge base ID")] public string KnowledgeBaseId { get; set; } = string.Empty;
    [Display("Category ID")] public string CategoryId { get; set; } = string.Empty;
    [Display("Author ID")] public string AuthorId { get; set; } = string.Empty;
    [Display("Language")] public string Language { get; set; } = string.Empty;
    [Display("Article type")] public string ArticleType { get; set; } = string.Empty;
    [Display("Body (HTML)")] public string Body { get; set; } = string.Empty;
    [Display("Created at")] public DateTime? CreatedAt { get; set; }
    [Display("Updated at")] public DateTime? UpdatedAt { get; set; }
}
