using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Identifiers;

public class ArticleIdentifier
{
    [Display("Article ID", Description = "The unique identifier (sys_id) of the knowledge article. You can get it from the 'Search articles' action.")]
    public string ArticleId { get; set; } = string.Empty;
}
