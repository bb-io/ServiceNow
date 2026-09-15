using Apps.ServiceNow.Handlers;
using Apps.ServiceNow.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Models.Requests;

public class ArticleCreatedOrUpdatedFilter
{
    [Display("Article ID", Description = "Only watch this single article.")]
    [DataSource(typeof(ArticleDataHandler))]
    public string? ArticleId { get; set; }

    [Display("Language", Description = "Only trigger for articles in this language. Leave empty for any language.")]
    [DataSource(typeof(LanguageDataHandler))]
    public string? Language { get; set; }

    [Display("Knowledge bases", Description = "Only trigger for articles that belong to one of the selected knowledge bases. Leave empty for any.")]
    [DataSource(typeof(KnowledgeBaseDataHandler))]
    public IEnumerable<string>? KnowledgeBaseIds { get; set; }

    [Display("Ignore translations", Description = "Only trigger for source articles. ServiceNow stores every translation as its own article, so with this off a translation written by 'Upload article' also triggers this event, which makes a translation flow trigger on its own output.")]
    public bool? IgnoreTranslations { get; set; }
    
    [Display("Tag IDs (all must be present)")]
    [DataSource(typeof(TagDataHandler))]
    public IEnumerable<string>? AllTagIds { get; set; }

    [Display("Tag IDs (at least one must be present)")]
    [DataSource(typeof(TagDataHandler))]
    public IEnumerable<string>? AnyTagIds { get; set; }

    [Display("Exclude tags", Description = "Skip articles carrying any of these tags. Takes precedence over the include filters.")]
    [DataSource(typeof(TagDataHandler))]
    public IEnumerable<string>? ExcludeTagIds { get; set; }
}

public class ArticleStatusChangedFilter
{
    [Display("Article ID", Description = "Only watch this single article.")]
    [DataSource(typeof(ArticleDataHandler))]
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

    [Display("Ignore translations", Description = "Only watch source articles. ServiceNow stores every translation as its own article with its own workflow state, so with this off a translation's state changes trigger this event too.")]
    public bool? IgnoreTranslations { get; set; }
}
