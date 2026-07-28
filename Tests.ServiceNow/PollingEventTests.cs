using Apps.ServiceNow.Actions;
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
    public async Task OnArticlesCreatedOrUpdated_LocalKindMemory_StillSeesAJustCreatedArticle()
    {
        // Memory round-trips through serialisation, so LastPollingTime can come back as a local time. Ahead of UTC
        // that pushes the 'since' bound into the future and the poll silently reports nothing.
        Assert.AreNotEqual(TimeSpan.Zero, TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow),
            "This test is only meaningful on a machine that is not on UTC.");

        var created = await new ArticleActions(InvocationContext, FileManager).CreateArticle(new CreateArticleRequest
        {
            Language = "en",
            Title = "Blackbird polling timezone probe",
            KnowledgeBaseId = "dfc19531bf2021003f07e2c1ac0739ab"
        });

        var since = DateTime.UtcNow.AddMinutes(-2);
        var result = await ArticleEvents.OnArticlesCreatedOrUpdated(
            WithMemory(since.ToLocalTime()), new ArticleCreatedOrUpdatedFilter());

        Console.WriteLine($"FlyBird={result.FlyBird}, count={result.Result?.TotalCount ?? 0}");
        Assert.IsTrue(result.FlyBird, "A local-kind memory timestamp must not push the polling window forward.");
        Assert.IsTrue(result.Result!.Items.Any(i => i.ContentId == created.ArticleId),
            "The article created moments ago has to be reported.");
    }

    [TestMethod]
    public async Task OnArticlesCreatedOrUpdated_IgnoreTranslations_ExcludesLanguageVariants()
    {
        var since = DateTime.UtcNow.AddDays(-3650);

        var all = await ArticleEvents.OnArticlesCreatedOrUpdated(
            WithMemory(since), new ArticleCreatedOrUpdatedFilter());
        var sourcesOnly = await ArticleEvents.OnArticlesCreatedOrUpdated(
            WithMemory(since), new ArticleCreatedOrUpdatedFilter { IgnoreTranslations = true });

        var allCount = all.Result?.TotalCount ?? 0;
        var sourceCount = sourcesOnly.Result?.TotalCount ?? 0;
        Console.WriteLine($"all={allCount} sourcesOnly={sourceCount}");

        Assert.IsTrue(sourceCount > 0);
        Assert.IsTrue(sourceCount < allCount,
            "Translations created by 'Upload article' must be excluded, otherwise a translation flow triggers on its own output.");
        Assert.IsTrue(sourcesOnly.Result!.Items.All(i => i.Language == "en"),
            "Only the English source articles should remain on this instance.");
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
