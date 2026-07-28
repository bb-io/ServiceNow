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
    public async Task ArticleHandler_ReturnsItems()
    {
        var handler = new ArticleDataHandler(InvocationContext);
        var items = (await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None)).ToList();
        foreach (var i in items) Console.WriteLine($"{i.Value}: {i.DisplayName}");
        Assert.IsTrue(items.Count > 0);
    }

    [TestMethod]
    public async Task IncidentHandler_ReturnsItems()
    {
        var handler = new IncidentDataHandler(InvocationContext);
        var items = (await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None)).ToList();
        foreach (var i in items) Console.WriteLine($"{i.Value}: {i.DisplayName}");

        Assert.IsTrue(items.Count > 0);
        Assert.IsTrue(items.Count <= 30, "The dropdown must stay capped.");
        Assert.IsTrue(items.All(i => !string.IsNullOrWhiteSpace(i.Value) && !string.IsNullOrWhiteSpace(i.DisplayName)));
        Assert.AreEqual(items.Count, items.Select(i => i.Value).Distinct().Count(), "Values must be unique.");
    }

    [TestMethod]
    public async Task IncidentHandler_SearchString_FiltersServerSide()
    {
        var handler = new IncidentDataHandler(InvocationContext);

        var all = (await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None)).ToList();
        var filtered = (await handler.GetDataAsync(
            new DataSourceContext { SearchString = "email" }, CancellationToken.None)).ToList();

        Console.WriteLine($"all={all.Count} filtered={filtered.Count}");
        Assert.IsTrue(filtered.Count > 0, "'email' should match incidents on the demo instance.");
        Assert.IsTrue(filtered.All(i => i.DisplayName.Contains("email", StringComparison.OrdinalIgnoreCase)),
            "Every hit has to match the search string in its number or short description.");
    }

    [TestMethod]
    public async Task AttachmentHandler_ReturnsOnlyIncidentAndArticleFiles()
    {
        var handler = new AttachmentDataHandler(InvocationContext);
        var items = (await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None)).ToList();
        foreach (var i in items) Console.WriteLine($"{i.Value}: {i.DisplayName}");

        Assert.IsTrue(items.Count > 0);
        Assert.IsTrue(items.Count <= 30, "The dropdown must stay capped.");
        Assert.AreEqual(items.Count, items.Select(i => i.Value).Distinct().Count(), "Values must be unique.");
        Assert.IsFalse(items.Any(i => i.DisplayName.Contains(".crl", StringComparison.OrdinalIgnoreCase)),
            "Platform-internal attachments must not reach the dropdown.");
        Assert.IsTrue(items.All(i => i.DisplayName.Contains('(') && i.DisplayName.EndsWith(')')),
            "Each file has to be labelled with the record it hangs off, so identical file names stay distinguishable.");
    }

    [TestMethod]
    public async Task AttachmentHandler_SearchString_FiltersByFileName()
    {
        var handler = new AttachmentDataHandler(InvocationContext);
        var items = (await handler.GetDataAsync(
            new DataSourceContext { SearchString = "bb" }, CancellationToken.None)).ToList();

        foreach (var i in items) Console.WriteLine($"{i.Value}: {i.DisplayName}");
        Assert.IsTrue(items.Count > 0, "The test attachments named bb*.txt should match.");
        Assert.IsTrue(items.All(i => i.DisplayName.Contains("bb", StringComparison.OrdinalIgnoreCase)));
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
