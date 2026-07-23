using Apps.ServiceNow.Handlers;
using Apps.ServiceNow.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Models.Requests;

public class ArticleCreatedOrUpdatedFilter
{
    [Display("Article ID", Description = "Only watch this single article (its sys_id). Leave empty to watch all articles.")]
    public string? ArticleId { get; set; }

    [Display("Language", Description = "Only trigger for articles in this language. Leave empty for any language.")]
    [DataSource(typeof(LanguageDataHandler))]
    public string? Language { get; set; }

    [Display("Knowledge bases", Description = "Only trigger for articles that belong to one of the selected knowledge bases. Leave empty for any.")]
    [DataSource(typeof(KnowledgeBaseDataHandler))]
    public IEnumerable<string>? KnowledgeBaseIds { get; set; }
}

public class ArticleStatusChangedFilter
{
    [Display("Article ID", Description = "Only watch this single article (its sys_id). Leave empty to watch all articles.")]
    public string? ArticleId { get; set; }

    [Display("Language", Description = "Only watch articles in this language. Leave empty for any language.")]
    [DataSource(typeof(LanguageDataHandler))]
    public string? Language { get; set; }

    [Display("Knowledge bases", Description = "Only watch articles that belong to one of the selected knowledge bases. Leave empty for any.")]
    [DataSource(typeof(KnowledgeBaseDataHandler))]
    public IEnumerable<string>? KnowledgeBaseIds { get; set; }

    [Display("Status", Description = "Only trigger when an article changes into one of these states (for example Published). Leave empty to trigger on any status change.")]
    [StaticDataSource(typeof(ArticleStateDataHandler))]
    public IEnumerable<string>? Statuses { get; set; }
}
