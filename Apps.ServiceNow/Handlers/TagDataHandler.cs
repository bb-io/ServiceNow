using Apps.ServiceNow.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.ServiceNow.Handlers;

public class TagDataHandler(InvocationContext invocationContext) : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var query = new List<string> { "viewable_by=everyone" };
        
        if (!string.IsNullOrEmpty(context.SearchString))
            query.Add($"nameLIKE{context.SearchString}");
        
        var request = new RestRequest("/api/now/table/label").AddQueryParameter("sysparm_query", string.Join('^', query));
        var response = await Client.ExecuteWithErrorHandling<ResultListWrapper<TagDto>>(request);

        return response.Result.Select(x => new DataSourceItem(x.SysId, string.IsNullOrWhiteSpace(x.Name) ? x.SysId : x.Name));
    }
}