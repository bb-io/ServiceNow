using Apps.ServiceNow.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Models.Requests;

public class DownloadAttachmentRequest
{
    [Display("Attachment ID", Description = "The attachment to download. Lists files attached to incidents and knowledge articles, newest first; start typing to search by file name.")]
    [DataSource(typeof(AttachmentDataHandler))]
    public string AttachmentId { get; set; } = string.Empty;
}
