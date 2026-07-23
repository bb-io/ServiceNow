using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.ServiceNow;

public class Application : IApplication, ICategoryProvider
{
    public string Name => "ServiceNow";

    public IEnumerable<ApplicationCategory> Categories
    {
        get =>
        [
            ApplicationCategory.ProjectManagementAndProductivity,
            ApplicationCategory.TaskManagement,
            ApplicationCategory.Cms
        ];
        set { }
    }

    public T GetInstance<T>() => throw new NotImplementedException();
}
