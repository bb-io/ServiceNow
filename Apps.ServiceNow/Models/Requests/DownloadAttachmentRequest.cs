using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Requests;

public class DownloadAttachmentRequest
{
    [Display("Attachment ID", Description = "The unique identifier (sys_id) of the attachment to download.")]
    public string AttachmentId { get; set; } = string.Empty;
}
