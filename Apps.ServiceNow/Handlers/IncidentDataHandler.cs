using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.ServiceNow.Handlers;

public class IncidentDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    private const int MaxItems = 30;

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context, CancellationToken cancellationToken)
    {
        var clauses = new List<string>();

        if (!string.IsNullOrWhiteSpace(context.SearchString))
            clauses.Add($"numberLIKE{context.SearchString}^ORshort_descriptionLIKE{context.SearchString}");

        clauses.Add("ORDERBYDESCsys_updated_on");

        var query = new Dictionary<string, string>
        {
            ["sysparm_fields"] = "sys_id,number,short_description",
            ["sysparm_query"] = string.Join("^", clauses)
        };

        var incidents = await Client.SearchTableAsync<IncidentDto>(ApiEndpoints.IncidentTable, query, MaxItems);

        return incidents
            .Where(x => !string.IsNullOrWhiteSpace(x.SysId))
            .Select(x => new DataSourceItem(x.SysId, BuildDisplayName(x)));
    }

    private static string BuildDisplayName(IncidentDto incident)
    {
        var hasNumber = !string.IsNullOrWhiteSpace(incident.Number);
        var hasDescription = !string.IsNullOrWhiteSpace(incident.ShortDescription);

        return (hasNumber, hasDescription) switch
        {
            (true, true) => $"{incident.Number} - {incident.ShortDescription}",
            (true, false) => incident.Number,
            (false, true) => incident.ShortDescription!,
            _ => incident.SysId
        };
    }
}
