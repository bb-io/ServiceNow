using Apps.ServiceNow.Events;
using Apps.ServiceNow.Models.Polling;
using Apps.ServiceNow.Models.Requests;
using Apps.ServiceNow.Models.Responses;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.ServiceNow.Base;

namespace Tests.ServiceNow;

[TestClass]
public class PollingEventTests : TestBase
{
    private ArticlePollingList ArticleEvents => new(InvocationContext);
    private IncidentPollingList IncidentEvents => new(InvocationContext);

    private static PollingEventRequest<PollingMemory> WithMemory(DateTime? since) =>
        new() { Memory = since is null ? null : new PollingMemory { LastPollingTime = since } };

    [TestMethod]
    public async Task OnArticlesCreatedOrUpdated_NullMemory_DoesNotFire()
    {
        var result = await ArticleEvents.OnArticlesCreatedOrUpdated(
            WithMemory(null), new ArticleCreatedOrUpdatedFilter());

        Assert.IsFalse(result.FlyBird, "The first poll must only establish a baseline.");
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Memory?.LastPollingTime, "Memory must be advanced on the first poll.");
    }

    [TestMethod]
    public async Task OnArticlesCreatedOrUpdated_PopulatedMemory_ReturnsRecentArticles()
    {
        var since = DateTime.UtcNow.AddDays(-3650);
        var result = await ArticleEvents.OnArticlesCreatedOrUpdated(
            WithMemory(since), new ArticleCreatedOrUpdatedFilter());

        Console.WriteLine($"FlyBird={result.FlyBird}, count={result.Result?.TotalCount ?? 0}");
        Assert.IsNotNull(result.Memory?.LastPollingTime);
        if (result.FlyBird)
        {
            Assert.IsNotNull(result.Result);
            Assert.IsTrue(result.Result!.Items.Count > 0);
            foreach (var a in result.Result.Items.Take(5))
                Console.WriteLine($"[{a.EventType}] {a.Number}: {a.Title} ({a.State})");
        }
    }

    [TestMethod]
    public async Task OnArticleStatusChanged_NullMemory_SnapshotsWithoutFiring()
    {
        var request = new PollingEventRequest<ArticleStatePollingMemory> { Memory = null };
        var result = await ArticleEvents.OnArticleStatusChanged(request, new ArticleStatusChangedFilter());

        Assert.IsFalse(result.FlyBird, "The first poll must only snapshot current states.");
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Memory);
        Assert.IsNotNull(result.Memory!.LastPollingTime);
        Console.WriteLine($"Snapshotted {result.Memory.ArticleStates.Count} article states.");
        Assert.IsTrue(result.Memory.ArticleStates.Count > 0, "Expected some articles to snapshot on the demo instance.");
    }

    [TestMethod]
    public async Task OnArticleStatusChanged_StaleStateInMemory_DetectsTheChange()
    {
        var seed = await ArticleEvents.OnArticleStatusChanged(
            new PollingEventRequest<ArticleStatePollingMemory> { Memory = null },
            new ArticleStatusChangedFilter());
        Assert.IsTrue(seed.Memory!.ArticleStates.Count > 0);

        var first = seed.Memory.ArticleStates.First();
        seed.Memory.ArticleStates[first.Key] = "__stale__";

        var result = await ArticleEvents.OnArticleStatusChanged(
            new PollingEventRequest<ArticleStatePollingMemory> { Memory = seed.Memory },
            new ArticleStatusChangedFilter());

        Console.WriteLine($"FlyBird={result.FlyBird}, changes={result.Result?.TotalCount ?? 0}");
        Assert.IsTrue(result.FlyBird, "A differing remembered state must be reported as a change.");
        Assert.IsNotNull(result.Result);
        Assert.IsTrue(result.Result!.Articles.Any(a => a.ArticleId == first.Key));
    }

    [TestMethod]
    public async Task OnNewIncidents_NullMemory_DoesNotFire()
    {
        var result = await IncidentEvents.OnNewIncidents(WithMemory(null));

        Assert.IsFalse(result.FlyBird);
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Memory?.LastPollingTime);
    }

    [TestMethod]
    public async Task OnNewIncidents_PopulatedMemory_ReturnsRecentIncidents()
    {
        var since = DateTime.UtcNow.AddDays(-3650);
        var result = await IncidentEvents.OnNewIncidents(WithMemory(since));

        Console.WriteLine($"FlyBird={result.FlyBird}, count={result.Result?.TotalCount ?? 0}");
        Assert.IsNotNull(result.Memory?.LastPollingTime);
        if (result.FlyBird)
        {
            Assert.IsNotNull(result.Result);
            Assert.IsTrue(result.Result!.Incidents.Count > 0);
        }
    }

    [TestMethod]
    public async Task OnNewIncidentComment_NullMemory_DoesNotFire()
    {
        var result = await IncidentEvents.OnNewIncidentComment(WithMemory(null));

        Assert.IsFalse(result.FlyBird);
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Memory?.LastPollingTime);
    }

    [TestMethod]
    public async Task OnNewIncidentComment_PopulatedMemory_ReturnsOnlyRecentComments()
    {
        var since = DateTime.UtcNow.AddDays(-3650);
        var result = await IncidentEvents.OnNewIncidentComment(WithMemory(since));

        Console.WriteLine($"FlyBird={result.FlyBird}, count={result.Result?.TotalCount ?? 0}");
        Assert.IsNotNull(result.Memory?.LastPollingTime);
        if (result.FlyBird)
        {
            Assert.IsNotNull(result.Result);
            foreach (var c in result.Result!.Comments.Take(5))
                Console.WriteLine($"{c.CreatedAt} incident={c.IncidentId} by={c.CreatedBy}: {c.Value}");
            Assert.IsTrue(result.Result.Comments.All(c => !string.IsNullOrWhiteSpace(c.IncidentId)));
        }
    }
}
