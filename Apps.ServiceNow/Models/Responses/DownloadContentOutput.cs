using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.ServiceNow.Models.Responses;

public class DownloadContentOutput : IDownloadContentOutput
{
    [Display("File", Description = "The self-describing translatable file (download) or the imported file (upload).")]
    public FileReference Content { get; set; } = default!;

    [Display("Root article ID", Description = "The sys_id of the main article the file describes.")]
    public string RootEntryId { get; set; } = string.Empty;

    [Display("Errors", Description = "Per-article problems encountered while processing. Empty when everything succeeded.")]
    public List<ContentProcessingError>? Errors { get; set; }
}

public class ContentProcessingError
{
    [Display("Article ID")] public string EntryId { get; set; } = string.Empty;
    [Display("Error")] public string ErrorMessage { get; set; } = string.Empty;
}
