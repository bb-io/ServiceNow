using Apps.ServiceNow.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.ServiceNow.Models.Requests;

public class UploadArticleRequest : IUploadContentInput
{
    [Display("File", Description = "The translated file to import. Supports files produced by the 'Download article' action.")]
    public FileReference Content { get; set; } = default!;

    [Display("Language", Description = "The language of the translation being uploaded.")]
    [DataSource(typeof(LanguageDataHandler))]
    public string Locale { get; set; } = string.Empty;

    [Display("Article ID", Description = "The article to write the translated fields into. Start typing to search by number or title. Leave empty to use the id embedded in the file.")]
    [DataSource(typeof(ArticleDataHandler))]
    public string? ContentId { get; set; }
}
