using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Responses;

public class SearchIncidentsResponse
{
    [Display("Incidents")] public List<IncidentResponse> Incidents { get; set; } = new();
    [Display("Total count", Description = "The number of incidents returned.")] public int TotalCount { get; set; }
}