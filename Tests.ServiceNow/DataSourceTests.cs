using Apps.ServiceNow.Handlers;
using Apps.ServiceNow.Handlers.Static;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.ServiceNow.Base;

namespace Tests.ServiceNow;

[TestClass]
public class DataSourceTests : TestBase
{
    [TestMethod]
    public async Task KnowledgeBaseHandler_ReturnsItems()
    {
        var handler = new KnowledgeBaseDataHandler(InvocationContext);
        var items = (await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None)).ToList();
        foreach (var i in items) Console.WriteLine($"{i.Value}: {i.DisplayName}");
        Assert.IsTrue(items.Count > 0);
    }

    [TestMethod]
    public async Task LanguageHandler_ReturnsItems()
    {
        var handler = new LanguageDataHandler(InvocationContext);
        var items = (await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None)).ToList();
        foreach (var i in items) Console.WriteLine($"{i.Value}: {i.DisplayName}");
        Assert.IsTrue(items.Count > 0);
    }

    [TestMethod]
    public async Task UserHandler_ReturnsItems()
    {
        var handler = new UserDataHandler(InvocationContext);
        var items = (await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None)).ToList();
        Console.WriteLine($"Users: {items.Count}");
        Assert.IsTrue(items.Count > 0);
    }

    [TestMethod]
    public void StaticHandlers_ReturnUniqueItems()
    {
        AssertUnique(new ArticleStateDataHandler().GetData());
        AssertUnique(new IncidentStateDataHandler().GetData());
        AssertUnique(new UrgencyDataHandler().GetData());
        AssertUnique(new ImpactDataHandler().GetData());
        AssertUnique(new PriorityDataHandler().GetData());
    }

    private static void AssertUnique(IEnumerable<DataSourceItem> items)
    {
        var list = items.ToList();
        Assert.IsTrue(list.Count > 0);
        Assert.AreEqual(list.Count, list.Select(x => x.Value).Distinct().Count(), "Static data source values must be unique.");
    }
}
