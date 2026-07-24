using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Handlers.Static;

public class ArticleStateDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("published", "Published"),
        new("draft", "Draft"),
        new("review", "Review"),
        new("retired", "Retired"),
    ];
}
