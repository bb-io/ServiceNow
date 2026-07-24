using Apps.ServiceNow.Handlers;
using Apps.ServiceNow.Handlers.Static;
using Apps.ServiceNow.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Models.Requests;

public class UpdateIncidentRequest
{    
    [Display("Incident ID", Description = "The unique identifier (sys_id) of the incident.")]
    public string IncidentId { get; set; } = string.Empty;
    
    [Display("Short description", Description = "A new short summary of the incident.")]
    public string? ShortDescription { get; set; }

    [Display("Description", Description = "A new detailed description.")]
    public string? Description { get; set; }

    [Display("State", Description = "The new state of the incident.")]
    [StaticDataSource(typeof(IncidentStateDataHandler))]
    public string? State { get; set; }

    [Display("Priority", Description = "The new priority of the incident.")]
    [StaticDataSource(typeof(PriorityDataHandler))]
    public string? Priority { get; set; }

    [Display("Urgency", Description = "The new urgency of the incident.")]
    [StaticDataSource(typeof(UrgencyDataHandler))]
    public string? Urgency { get; set; }

    [Display("Impact", Description = "The new impact of the incident.")]
    [StaticDataSource(typeof(ImpactDataHandler))]
    public string? Impact { get; set; }

    [Display("Assigned to ID", Description = "The user the incident is assigned to.")]
    [DataSource(typeof(UserDataHandler))]
    public string? AssignedToId { get; set; }
}
