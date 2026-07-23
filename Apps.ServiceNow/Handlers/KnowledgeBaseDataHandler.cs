using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.ServiceNow.Handlers;

public class KnowledgeBaseDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string> { ["sysparm_fields"] = "sys_id,title" };
        if (!string.IsNullOrWhiteSpace(context.SearchString))
            query["sysparm_query"] = $"titleLIKE{context.SearchString}";

        var bases = await Client.SearchTableAsync<KnowledgeBaseItemDto>(ApiEndpoints.KnowledgeBaseTable, query, 30);

        return bases
            .Where(x => !string.IsNullOrWhiteSpace(x.SysId) && !string.IsNullOrWhiteSpace(x.Title))
            .Select(x => new DataSourceItem(x.SysId!, x.Title!));
    }
}
