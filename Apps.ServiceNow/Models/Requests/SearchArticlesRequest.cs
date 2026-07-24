using Apps.ServiceNow.Handlers;
using Apps.ServiceNow.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Models.Requests;

public class SearchArticlesRequest
{
    [Display("Search text", Description = "Words to look for in the article title and body. Leave empty to match all articles.")]
    public string? Query { get; set; }

    [Display("Language", Description = "Only return articles in this language. Defaults to English when left empty.")]
    [DataSource(typeof(LanguageDataHandler))]
    public string? Language { get; set; }

    [Display("Knowledge bases", Description = "Only return articles that belong to one of the selected knowledge bases.")]
    [DataSource(typeof(KnowledgeBaseDataHandler))]
    public IEnumerable<string>? KnowledgeBaseIds { get; set; }

    [Display("State", Description = "Only return articles in this state, for example Published or Draft.")]
    [StaticDataSource(typeof(ArticleStateDataHandler))]
    public string? State { get; set; }

    [Display("Created after", Description = "Only return articles created on or after this date (UTC).")]
    public DateTime? CreatedAfter { get; set; }

    [Display("Created before", Description = "Only return articles created on or before this date (UTC).")]
    public DateTime? CreatedBefore { get; set; }

    [Display("Updated after", Description = "Only return articles last updated on or after this date (UTC).")]
    public DateTime? UpdatedAfter { get; set; }

    [Display("Updated before", Description = "Only return articles last updated on or before this date (UTC).")]
    public DateTime? UpdatedBefore { get; set; }

    [Display("Maximum results", Description = "Optional cap on how many articles to return. Leave empty to return all matches.")]
    public int? Limit { get; set; }
}
