using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.ServiceNow.Handlers;

public class UserDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string>
        {
            ["sysparm_fields"] = "sys_id,name,user_name",
            ["sysparm_query"] = "active=true^ORDERBYname"
        };
        if (!string.IsNullOrWhiteSpace(context.SearchString))
            query["sysparm_query"] = $"active=true^nameLIKE{context.SearchString}^ORDERBYname";

        var users = await Client.SearchTableAsync<UserItemDto>(ApiEndpoints.UserTable, query, 30);

        return users
            .Where(x => !string.IsNullOrWhiteSpace(x.SysId))
            .Select(x => new DataSourceItem(x.SysId, string.IsNullOrWhiteSpace(x.Name) ? x.UserName ?? x.SysId : x.Name!));
    }
}
