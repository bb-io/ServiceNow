using Apps.ServiceNow.Handlers;
using Apps.ServiceNow.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Models.Requests;

public class UpdateArticleRequest : ArticleIdentifier
{
    [Display("Title", Description = "A new title for the article.")]
    public string? Title { get; set; }

    [Display("Body (HTML)", Description = "The new article body, as HTML.")]
    public string? Body { get; set; }

    [Display("Knowledge base ID", Description = "Move the article to a different knowledge base.")]
    [DataSource(typeof(KnowledgeBaseDataHandler))]
    public string? KnowledgeBaseId { get; set; }

    [Display("Category ID", Description = "The category (sys_id) to assign the article to.")]
    public string? CategoryId { get; set; }

    [Display("Language", Description = "The language of the article.")]
    [DataSource(typeof(LanguageDataHandler))]
    public string? Language { get; set; }
}
