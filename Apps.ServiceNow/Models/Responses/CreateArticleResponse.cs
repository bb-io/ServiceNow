using Apps.ServiceNow.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Responses;

public class CreateArticleResponse
{
    public CreateArticleResponse() { }

    public CreateArticleResponse(ArticleDto dto)
    {
        ArticleId = dto.SysId;
        Number = dto.Number;
        Title = dto.ShortDescription ?? string.Empty;
        State = dto.WorkflowState ?? string.Empty;
    }

    [Display("Article ID")] public string ArticleId { get; set; } = string.Empty;
    [Display("Number")] public string Number { get; set; } = string.Empty;
    [Display("Title")] public string Title { get; set; } = string.Empty;
    [Display("State", Description = "The workflow state of the new article. Newly created articles are drafts.")] public string State { get; set; } = string.Empty;
}
