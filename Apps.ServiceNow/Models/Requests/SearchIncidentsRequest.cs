using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Requests;

public class SearchIncidentsRequest
{
    [Display("Query", Description = "A ServiceNow encoded query, for example state=2^priority=1. Leave empty to return all incidents.")]
    public string? Query { get; set; }

    [Display("Maximum results", Description = "Optional cap on how many incidents to return. Leave empty to return all matches.")]
    public int? Limit { get; set; }
}
