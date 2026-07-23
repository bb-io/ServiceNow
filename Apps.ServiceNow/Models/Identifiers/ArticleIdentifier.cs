using Apps.ServiceNow.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Models.Identifiers;

public class ArticleIdentifier
{
    [Display("Article ID", Description = "The knowledge article to use. Start typing to search by number or title.")]
    [DataSource(typeof(ArticleDataHandler))]
    public string ArticleId { get; set; } = string.Empty;
}
