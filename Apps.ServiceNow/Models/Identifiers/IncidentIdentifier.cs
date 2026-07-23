using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Identifiers;

public class IncidentIdentifier
{
    [Display("Incident ID", Description = "The unique identifier (sys_id) of the incident. You can get it from the 'Search incidents' or 'Create incident' action.")]
    public string IncidentId { get; set; } = string.Empty;
}
