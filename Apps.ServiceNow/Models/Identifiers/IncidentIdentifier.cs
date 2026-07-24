using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Identifiers;

public class IncidentIdentifier
{
    [Display("Incident ID", Description = "The unique identifier (sys_id) of the incident.")]
    public string IncidentId { get; set; } = string.Empty;
}
