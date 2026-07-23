using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Handlers.Static;

public class UrgencyDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("1", "High"),
        new("2", "Medium"),
        new("3", "Low"),
    ];
}

public class ImpactDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("1", "High"),
        new("2", "Medium"),
        new("3", "Low"),
    ];
}

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
