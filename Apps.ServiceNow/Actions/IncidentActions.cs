using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Extensions;
using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Models.Identifiers;
using Apps.ServiceNow.Models.Requests;
using Apps.ServiceNow.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.ServiceNow.Actions;

[ActionList("Incidents")]
public class IncidentActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    private const string IncidentFields = TableFields.Incident;

    [Action("Create incident", Description = "Open a new incident.")]
    public async Task<IncidentResponse> CreateIncident([ActionParameter] CreateIncidentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ShortDescription))
            throw new PluginMisconfigurationException("Please fill in the 'Short description' field.");

        var body = new Dictionary<string, object> { ["short_description"] = request.ShortDescription }
            .AddIfNotNull("description", request.Description)
            .AddIfNotEmpty("urgency", request.Urgency)
            .AddIfNotEmpty("impact", request.Impact)
            .AddIfNotEmpty("caller_id", request.CallerId)
            .AddIfNotEmpty("assigned_to", request.AssignedToId);

        var dto = await Client.CreateRecordAsync<IncidentDto>(ApiEndpoints.IncidentTable, body,
            new Dictionary<string, string> { ["sysparm_fields"] = IncidentFields });
        return new IncidentResponse(dto);
    }

    [Action("Get incident", Description = "Retrieve a single incident by its ID.")]
    public async Task<IncidentResponse> GetIncident([ActionParameter] IncidentIdentifier identifier)
    {
        ValidateIncidentId(identifier.IncidentId);
        var dto = await Client.GetRecordAsync<IncidentDto>(ApiEndpoints.IncidentTable, identifier.IncidentId,
            new Dictionary<string, string> { ["sysparm_fields"] = IncidentFields });
        return new IncidentResponse(dto);
    }

    [Action("Update incident", Description = "Change one or more fields on an existing incident.")]
    public async Task<IncidentResponse> UpdateIncident([ActionParameter] UpdateIncidentRequest request)
    {
        ValidateIncidentId(request.IncidentId);

        var body = new Dictionary<string, object>()
            .AddIfNotNull("short_description", request.ShortDescription)
            .AddIfNotNull("description", request.Description)
            .AddIfNotEmpty("state", request.State)
            .AddIfNotEmpty("priority", request.Priority)
            .AddIfNotEmpty("urgency", request.Urgency)
            .AddIfNotEmpty("impact", request.Impact)
            .AddIfNotEmpty("assigned_to", request.AssignedToId);

        if (body.Count == 0)
            throw new PluginMisconfigurationException(
                "No changes were provided. Fill in at least one field to update on the incident.");

        var dto = await Client.UpdateRecordAsync<IncidentDto>(ApiEndpoints.IncidentTable, request.IncidentId, body,
            new Dictionary<string, string> { ["sysparm_fields"] = IncidentFields });
        return new IncidentResponse(dto);
    }

    [Action("Delete incident", Description = "Permanently remove an incident.")]
    public async Task<DeleteIncidentResponse> DeleteIncident([ActionParameter] IncidentIdentifier identifier)
    {
        ValidateIncidentId(identifier.IncidentId);
        await Client.DeleteRecordAsync(ApiEndpoints.IncidentTable, identifier.IncidentId);
        return new DeleteIncidentResponse { Success = true };
    }

    [Action("Search incidents", Description = "Find incidents matching an encoded query.")]
    public async Task<SearchIncidentsResponse> SearchIncidents([ActionParameter] SearchIncidentsRequest request)
    {
        if (request.Limit is <= 0)
            throw new PluginMisconfigurationException("The 'Maximum results' value must be greater than zero.");

        var query = new Dictionary<string, string> { ["sysparm_fields"] = IncidentFields };
        if (!string.IsNullOrWhiteSpace(request.Query))
            query["sysparm_query"] = request.Query;

        var dtos = await Client.SearchTableAsync<IncidentDto>(ApiEndpoints.IncidentTable, query, request.Limit);
        var incidents = dtos.Select(x => new IncidentResponse(x)).ToList();
        return new SearchIncidentsResponse { Incidents = incidents, TotalCount = incidents.Count };
    }

    [Action("Get incident comments", Description = "List the customer-visible comments on an incident, newest first.")]
    public async Task<CommentsResponse> GetIncidentComments([ActionParameter] IncidentIdentifier identifier)
    {
        ValidateIncidentId(identifier.IncidentId);

        var query = new Dictionary<string, string>
        {
            ["sysparm_query"] = $"element_id={identifier.IncidentId}^element=comments^ORDERBYDESCsys_created_on",
            ["sysparm_fields"] = "sys_id,sys_created_on,sys_created_by,value,element"
        };

        var entries = await Client.SearchTableAsync<JournalEntryDto>(ApiEndpoints.JournalTable, query);
        return new CommentsResponse { Comments = entries.Select(x => new CommentItem(x)).ToList() };
    }

    [Action("Add comment to incident", Description = "Append a customer-visible comment to an incident.")]
    public async Task<IncidentReferenceResponse> AddCommentToIncident([ActionParameter] AddCommentRequest request)
    {
        ValidateIncidentId(request.IncidentId);
        if (string.IsNullOrWhiteSpace(request.Comment))
            throw new PluginMisconfigurationException("Please fill in the 'Comment' field.");

        var body = new Dictionary<string, object> { ["comments"] = request.Comment };
        var dto = await Client.UpdateRecordAsync<IncidentDto>(ApiEndpoints.IncidentTable, request.IncidentId, body,
            new Dictionary<string, string> { ["sysparm_fields"] = "sys_id,number" });

        return new IncidentReferenceResponse { IncidentId = dto.SysId, Number = dto.Number };
    }

    private static void ValidateIncidentId(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            throw new PluginMisconfigurationException("Please fill in the 'Incident ID' field.");
    }
}
