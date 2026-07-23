using Apps.ServiceNow.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Models.Requests;

public class CreateArticleRequest
{
    [Display("Language", Description = "The language of the article, for example English.")]
    [DataSource(typeof(LanguageDataHandler))]
    public string Language { get; set; } = string.Empty;

    [Display("Title", Description = "The title of the article.")]
    public string Title { get; set; } = string.Empty;

    [Display("Knowledge base ID", Description = "The knowledge base the article should live in.")]
    [DataSource(typeof(KnowledgeBaseDataHandler))]
    public string KnowledgeBaseId { get; set; } = string.Empty;

    [Display("Content (HTML)", Description = "An optional article body, as HTML.")]
    public string? Content { get; set; }
}
