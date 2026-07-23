using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Utils;
using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Responses;

public class CommentItem
{
    public CommentItem() { }

    public CommentItem(JournalEntryDto dto)
    {
        CommentId = dto.SysId;
        Value = dto.Value ?? string.Empty;
        CreatedBy = dto.CreatedBy ?? string.Empty;
        CreatedAt = ServiceNowDate.Parse(dto.CreatedOn);
    }

    [Display("Comment ID")] public string CommentId { get; set; } = string.Empty;
    [Display("Comment", Description = "The text of the comment.")] public string Value { get; set; } = string.Empty;
    [Display("Created by", Description = "The username of the comment author.")] public string CreatedBy { get; set; } = string.Empty;
    [Display("Created at")] public DateTime? CreatedAt { get; set; }
}

public class CommentsResponse
{
    [Display("Comments")] public List<CommentItem> Comments { get; set; } = new();
}
