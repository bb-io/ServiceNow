using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Handlers.Static;

public class ImpactDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("1", "High"),
        new("2", "Medium"),
        new("3", "Low"),
    ];
}
