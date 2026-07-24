using Apps.ServiceNow.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Requests;

public class AddCommentRequest : IncidentIdentifier
{
    [Display("Comment", Description = "The customer-visible comment to add to the incident.")]
    public string Comment { get; set; } = string.Empty;
}
