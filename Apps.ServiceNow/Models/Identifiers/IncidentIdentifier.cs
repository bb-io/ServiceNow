using Apps.ServiceNow.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Models.Identifiers;

public class IncidentIdentifier
{
    [Display("Incident ID", Description = "The incident to act on.")]
    [DataSource(typeof(IncidentDataHandler))]
    public string IncidentId { get; set; } = string.Empty;
}
