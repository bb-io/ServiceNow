using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.ServiceNow.Handlers;

public class ArticleDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string>
        {
            ["sysparm_fields"] = "sys_id,number,short_description",
            ["sysparm_query"] = "ORDERBYDESCsys_updated_on"
        };

        if (!string.IsNullOrWhiteSpace(context.SearchString))
            query["sysparm_query"] =
                $"short_descriptionLIKE{context.SearchString}^ORnumberLIKE{context.SearchString}^ORDERBYDESCsys_updated_on";

        var articles = await Client.SearchTableAsync<ArticleItemDto>(ApiEndpoints.KnowledgeTable, query, 30);

        return articles
            .Where(x => !string.IsNullOrWhiteSpace(x.SysId))
            .Select(x => new DataSourceItem(x.SysId!, BuildDisplayName(x)));
    }

    private static string BuildDisplayName(ArticleItemDto article)
    {
        var hasNumber = !string.IsNullOrWhiteSpace(article.Number);
        var hasTitle = !string.IsNullOrWhiteSpace(article.ShortDescription);

        return (hasNumber, hasTitle) switch
        {
            (true, true) => $"{article.Number} - {article.ShortDescription}",
            (true, false) => article.Number!,
            (false, true) => article.ShortDescription!,
            _ => article.SysId!
        };
    }
}
