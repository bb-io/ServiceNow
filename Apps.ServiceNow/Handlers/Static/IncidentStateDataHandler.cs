using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Handlers.Static;

public class IncidentStateDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("1", "New"),
        new("2", "In progress"),
        new("3", "On hold"),
        new("6", "Resolved"),
        new("7", "Closed"),
        new("8", "Canceled"),
    ];
}
