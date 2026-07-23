using Apps.ServiceNow.Handlers;
using Apps.ServiceNow.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Models.Requests;

public class CreateIncidentRequest
{
    [Display("Short description", Description = "A short summary of the incident.")]
    public string ShortDescription { get; set; } = string.Empty;

    [Display("Description", Description = "A detailed description of the incident.")]
    public string? Description { get; set; }

    [Display("Urgency", Description = "How urgent the incident is.")]
    [StaticDataSource(typeof(UrgencyDataHandler))]
    public string? Urgency { get; set; }

    [Display("Impact", Description = "The impact of the incident.")]
    [StaticDataSource(typeof(ImpactDataHandler))]
    public string? Impact { get; set; }

    [Display("Caller ID", Description = "The user who reported the incident.")]
    [DataSource(typeof(UserDataHandler))]
    public string? CallerId { get; set; }

    [Display("Assigned to ID", Description = "The user the incident is assigned to.")]
    [DataSource(typeof(UserDataHandler))]
    public string? AssignedToId { get; set; }
}
