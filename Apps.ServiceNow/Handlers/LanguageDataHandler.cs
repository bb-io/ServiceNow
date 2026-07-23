using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.ServiceNow.Handlers;

public class LanguageDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string>
        {
            ["sysparm_query"] = "inactive=false",
            ["sysparm_fields"] = "name,id"
        };

        var languages = await Client.SearchTableAsync<LanguageItemDto>(ApiEndpoints.LanguageTable, query);

        return languages
            .Where(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Name))
            .Where(x => string.IsNullOrWhiteSpace(context.SearchString)
                        || x.Name!.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)
                        || x.Id!.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .Select(x => new DataSourceItem(x.Id!, x.Name!));
    }
}
