using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Utils;
using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Responses;

public class IncidentResponse
{
    public IncidentResponse() { }

    public IncidentResponse(IncidentDto dto)
    {
        IncidentId = dto.SysId;
        Number = dto.Number;
        ShortDescription = dto.ShortDescription ?? string.Empty;
        Description = dto.Description ?? string.Empty;
        State = dto.State ?? string.Empty;
        Priority = dto.Priority ?? string.Empty;
        Urgency = dto.Urgency ?? string.Empty;
        Impact = dto.Impact ?? string.Empty;
        CallerId = dto.CallerId?.Value ?? string.Empty;
        AssignedToId = dto.AssignedTo?.Value ?? string.Empty;
        CreatedAt = ServiceNowDate.Parse(dto.CreatedOn);
        UpdatedAt = ServiceNowDate.Parse(dto.UpdatedOn);
    }

    [Display("Incident ID")] public string IncidentId { get; set; } = string.Empty;
    [Display("Number", Description = "The human-readable incident number, for example INC0010001.")] public string Number { get; set; } = string.Empty;
    [Display("Short description")] public string ShortDescription { get; set; } = string.Empty;
    [Display("Description")] public string Description { get; set; } = string.Empty;
    [Display("State", Description = "The state code of the incident, for example 2 for In progress.")] public string State { get; set; } = string.Empty;
    [Display("Priority", Description = "The priority code, derived from urgency and impact.")] public string Priority { get; set; } = string.Empty;
    [Display("Urgency")] public string Urgency { get; set; } = string.Empty;
    [Display("Impact")] public string Impact { get; set; } = string.Empty;
    [Display("Caller ID")] public string CallerId { get; set; } = string.Empty;
    [Display("Assigned to ID")] public string AssignedToId { get; set; } = string.Empty;
    [Display("Created at")] public DateTime? CreatedAt { get; set; }
    [Display("Updated at")] public DateTime? UpdatedAt { get; set; }
}
