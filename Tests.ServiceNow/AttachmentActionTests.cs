using System.Text;
using Apps.ServiceNow.Actions;
using Apps.ServiceNow.Api;
using Apps.ServiceNow.Models.Identifiers;
using Apps.ServiceNow.Models.Requests;
using RestSharp;
using Tests.ServiceNow.Base;

namespace Tests.ServiceNow;

[TestClass]
public class AttachmentActionTests : TestBase
{
    [TestMethod]
    public async Task DownloadAttachment_ValidId_ReturnsBytes()
    {
        var incidentActions = new IncidentActions(InvocationContext);
        var attachmentActions = new AttachmentActions(InvocationContext, FileManager);
        var client = new Client(Creds.ToArray());

        var incident = await incidentActions.CreateIncident(new CreateIncidentRequest
        {
            ShortDescription = "Blackbird attachment test incident"
        });

        try
        {
            const string content = "Hello attachment world";
            var uploadRequest = new RestRequest("/api/now/attachment/file", Method.Post)
                .AddQueryParameter("table_name", "incident")
                .AddQueryParameter("table_sys_id", incident.IncidentId)
                .AddQueryParameter("file_name", "bb_attach.txt");
            uploadRequest.AddHeader("Content-Type", "text/plain");
            uploadRequest.AddParameter("text/plain", Encoding.UTF8.GetBytes(content), ParameterType.RequestBody);
            var uploadResponse = await client.ExecuteAsync(uploadRequest);
            Assert.IsTrue(uploadResponse.IsSuccessStatusCode, $"Attachment upload failed: {uploadResponse.StatusCode}");

            var attachments = await client.ListAttachmentsAsync("incident", incident.IncidentId);
            Assert.IsTrue(attachments.Count > 0, "Expected at least one attachment on the incident.");
            var attachmentId = attachments[0].SysId;

            var result = await attachmentActions.DownloadAttachment(new DownloadAttachmentRequest
            {
                AttachmentId = attachmentId
            });

            Console.WriteLine($"Downloaded {result.File?.Name}, {result.FileSize} bytes, type {result.ContentType}");
            Assert.IsNotNull(result.File);
            Assert.IsTrue(result.FileSize > 0);
        }
        finally
        {
            await incidentActions.DeleteIncident(new IncidentIdentifier { IncidentId = incident.IncidentId });
        }
    }
}
