using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.ServiceNow.Models.Responses;

public class DownloadAttachmentResponse
{
    [Display("File", Description = "The downloaded attachment.")]
    public FileReference File { get; set; } = default!;

    [Display("Content type", Description = "The MIME type of the file, for example text/plain.")]
    public string ContentType { get; set; } = string.Empty;

    [Display("File size", Description = "The size of the file in bytes.")]
    public long FileSize { get; set; }
}
