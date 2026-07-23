using Apps.ServiceNow.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.ServiceNow.Models.Requests;

public class DownloadArticleRequest : IDownloadContentInput
{
    [Display("Article ID", Description = "The unique identifier (sys_id) of the knowledge article to download. You can get it from the 'Search articles' action.")]
    public string ContentId { get; set; } = string.Empty;

    [Display("Language", Description = "The language version to download. Defaults to the article's own language when left empty.")]
    [DataSource(typeof(LanguageDataHandler))]
    public string? Locale { get; set; }
}
