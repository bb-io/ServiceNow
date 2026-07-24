using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Handlers.Static;

public class PriorityDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("1", "Critical"),
        new("2", "High"),
        new("3", "Moderate"),
        new("4", "Low"),
        new("5", "Planning"),
    ];
}
