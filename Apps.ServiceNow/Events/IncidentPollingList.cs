using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Models.Polling;
using Apps.ServiceNow.Models.Responses;
using Apps.ServiceNow.Utils;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;

namespace Apps.ServiceNow.Events;

[PollingEventList]
public class IncidentPollingList(InvocationContext invocationContext) : Invocable(invocationContext)
{
    private const string IncidentFields = TableFields.Incident;

    [PollingEvent("On new incidents",
        Description = "Triggered on an interval and outputs incidents opened since the previous poll.")]
    public async Task<PollingEventResponse<PollingMemory, IncidentsEventResponse>> OnNewIncidents(
        PollingEventRequest<PollingMemory> request)
    {
        if (request.Memory?.LastPollingTime is null)
            return Baseline<IncidentsEventResponse>();

        var since = request.Memory.LastPollingTime.Value;

        var query = new Dictionary<string, string>
        {
            ["sysparm_query"] = $"sys_created_on>{ServiceNowDate.Format(since)}^ORDERBYsys_created_on",
            ["sysparm_fields"] = IncidentFields
        };

        var dtos = await Client.SearchTableAsync<IncidentDto>(ApiEndpoints.IncidentTable, query);
        var incidents = dtos.Select(x => new IncidentResponse(x)).ToList();

        return new PollingEventResponse<PollingMemory, IncidentsEventResponse>
        {
            FlyBird = incidents.Count > 0,
            Memory = new PollingMemory { LastPollingTime = DateTime.UtcNow },
            Result = incidents.Count > 0
                ? new IncidentsEventResponse { Incidents = incidents, TotalCount = incidents.Count }
                : null
        };
    }

    [PollingEvent("On new incident comment",
        Description = "Triggered on an interval and outputs customer-visible incident comments added since the previous poll.")]
    public async Task<PollingEventResponse<PollingMemory, IncidentCommentsEventResponse>> OnNewIncidentComment(
        PollingEventRequest<PollingMemory> request)
    {
        if (request.Memory?.LastPollingTime is null)
            return Baseline<IncidentCommentsEventResponse>();

        var since = request.Memory.LastPollingTime.Value;

        var query = new Dictionary<string, string>
        {
            ["sysparm_query"] =
                $"name=incident^element=comments^sys_created_on>{ServiceNowDate.Format(since)}^ORDERBYsys_created_on",
            ["sysparm_fields"] = "sys_id,sys_created_on,sys_created_by,value,element,name,element_id"
        };

        var entries = await Client.SearchTableAsync<JournalEntryDto>(ApiEndpoints.JournalTable, query);
        var comments = entries.Select(x => new IncidentCommentEventItem(x)).ToList();

        return new PollingEventResponse<PollingMemory, IncidentCommentsEventResponse>
        {
            FlyBird = comments.Count > 0,
            Memory = new PollingMemory { LastPollingTime = DateTime.UtcNow },
            Result = comments.Count > 0
                ? new IncidentCommentsEventResponse { Comments = comments, TotalCount = comments.Count }
                : null
        };
    }
}
