using Apps.ServiceNow.Models.Requests;
using Apps.ServiceNow.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.ServiceNow.Actions;

[ActionList("Attachments")]
public class AttachmentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : Invocable(invocationContext)
{
    [Action("Download attachment", Description = "Download the file content of an attachment.")]
    public async Task<DownloadAttachmentResponse> DownloadAttachment([ActionParameter] DownloadAttachmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AttachmentId))
            throw new PluginMisconfigurationException("Please fill in the 'Attachment ID' field.");

        var metadata = await Client.GetAttachmentMetadataAsync(request.AttachmentId);
        var (bytes, contentType) = await Client.DownloadAttachmentAsync(request.AttachmentId);

        var fileName = string.IsNullOrWhiteSpace(metadata.FileName) ? request.AttachmentId : metadata.FileName!;
        var effectiveContentType = !string.IsNullOrWhiteSpace(metadata.ContentType)
            ? metadata.ContentType!
            : contentType;

        var file = await fileManagementClient.UploadAsync(new MemoryStream(bytes), effectiveContentType, fileName);

        return new DownloadAttachmentResponse
        {
            File = file,
            ContentType = effectiveContentType,
            FileSize = bytes.LongLength
        };
    }
}
