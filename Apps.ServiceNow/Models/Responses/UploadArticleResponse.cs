using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Responses;

public class UploadArticleResponse : DownloadContentOutput
{
    [Display("Translated article ID",
        Description = "The sys_id of the language variant the translation was written to. Differs from the root article ID whenever the upload targets another language, because ServiceNow keeps each language in its own article.")]
    public string TargetEntryId { get; set; } = string.Empty;
}
