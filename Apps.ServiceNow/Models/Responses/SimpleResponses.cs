using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Responses;

public class DeleteIncidentResponse
{
    [Display("Success", Description = "True when the incident was removed.")]
    public bool Success { get; set; }
}

public class IncidentReferenceResponse
{
    [Display("Incident ID")] public string IncidentId { get; set; } = string.Empty;
    [Display("Number")] public string Number { get; set; } = string.Empty;
}
