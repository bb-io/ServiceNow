using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Utils;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Responses;

public class ArticleEventItem : ArticleMetadataResponse
{
    public ArticleEventItem() { }

    public ArticleEventItem(ArticleDto dto, string eventType) : base(dto)
    {
        EventType = eventType;
    }

    [Display("Event type", Description = "Either created or updated.")]
    public string EventType { get; set; } = string.Empty;
}

public class ArticlesEventResponse : IMultiDownloadableContentOutput<ArticleEventItem>
{
    [Display("Articles")] public List<ArticleEventItem> Items { get; set; } = new();
    [Display("Total count", Description = "The number of articles in this event.")] public int TotalCount { get; set; }
}

public class ArticleStatusChangeItem
{
    public ArticleStatusChangeItem() { }

    public ArticleStatusChangeItem(ArticleDto dto, string previousState)
    {
        ArticleId = dto.SysId;
        Number = dto.Number;
        Title = dto.ShortDescription ?? string.Empty;
        PreviousState = previousState;
        NewState = dto.WorkflowState ?? string.Empty;
        Language = dto.Language ?? string.Empty;
        KnowledgeBaseId = dto.KnowledgeBase?.Value ?? string.Empty;
        UpdatedAt = ServiceNowDate.Parse(dto.UpdatedOn);
    }

    [Display("Article ID")] public string ArticleId { get; set; } = string.Empty;
    [Display("Number", Description = "The human-readable article number, for example KB0000024.")] public string Number { get; set; } = string.Empty;
    [Display("Title")] public string Title { get; set; } = string.Empty;
    [Display("Previous state", Description = "The workflow state at the previous poll.")] public string PreviousState { get; set; } = string.Empty;
    [Display("New state", Description = "The workflow state now, for example published or draft.")] public string NewState { get; set; } = string.Empty;
    [Display("Language")] public string Language { get; set; } = string.Empty;
    [Display("Knowledge base ID")] public string KnowledgeBaseId { get; set; } = string.Empty;
    [Display("Updated at")] public DateTime? UpdatedAt { get; set; }
}

public class ArticleStatusChangedEventResponse
{
    [Display("Articles")] public List<ArticleStatusChangeItem> Articles { get; set; } = new();
    [Display("Total count", Description = "The number of articles whose status changed.")] public int TotalCount { get; set; }
}

public class IncidentsEventResponse
{
    [Display("Incidents")] public List<IncidentResponse> Incidents { get; set; } = new();
    [Display("Total count", Description = "The number of new incidents in this event.")] public int TotalCount { get; set; }
}

public class IncidentCommentEventItem
{
    public IncidentCommentEventItem() { }

    public IncidentCommentEventItem(JournalEntryDto dto)
    {
        CommentId = dto.SysId;
        IncidentId = dto.ElementId ?? string.Empty;
        Value = dto.Value ?? string.Empty;
        CreatedBy = dto.CreatedBy ?? string.Empty;
        CreatedAt = ServiceNowDate.Parse(dto.CreatedOn);
    }

    [Display("Comment ID")] public string CommentId { get; set; } = string.Empty;
    [Display("Incident ID", Description = "The sys_id of the incident this comment was added to.")] public string IncidentId { get; set; } = string.Empty;
    [Display("Comment", Description = "The text of the comment.")] public string Value { get; set; } = string.Empty;
    [Display("Created by", Description = "The username of the comment author.")] public string CreatedBy { get; set; } = string.Empty;
    [Display("Created at")] public DateTime? CreatedAt { get; set; }
}

public class IncidentCommentsEventResponse
{
    [Display("Comments")] public List<IncidentCommentEventItem> Comments { get; set; } = new();
    [Display("Total count", Description = "The number of new comments in this event.")] public int TotalCount { get; set; }
}
