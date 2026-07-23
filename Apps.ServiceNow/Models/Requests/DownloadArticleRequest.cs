using Apps.ServiceNow.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.ServiceNow.Models.Requests;

public class DownloadArticleRequest : IDownloadContentInput
{
    [Display("Article ID", Description = "The knowledge article to download. Start typing to search by number or title.")]
    [DataSource(typeof(ArticleDataHandler))]
    public string ContentId { get; set; } = string.Empty;

    [Display("Language", Description = "The language version to download. Defaults to the article's own language when left empty.")]
    [DataSource(typeof(LanguageDataHandler))]
    public string? Locale { get; set; }
}
