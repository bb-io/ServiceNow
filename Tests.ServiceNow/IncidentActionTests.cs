using Apps.ServiceNow.Actions;
using Apps.ServiceNow.Models.Identifiers;
using Apps.ServiceNow.Models.Requests;
using Tests.ServiceNow.Base;

namespace Tests.ServiceNow;

[TestClass]
public class IncidentActionTests : TestBase
{
    private IncidentActions Actions => new(InvocationContext);

    private async Task<string> CreateDisposableIncidentAsync(string summary = "Blackbird integration test incident")
    {
        var created = await Actions.CreateIncident(new CreateIncidentRequest
        {
            ShortDescription = summary,
            Description = "Created by an integration test",
            Urgency = "2",
            Impact = "2"
        });
        Assert.IsFalse(string.IsNullOrWhiteSpace(created.IncidentId));
        return created.IncidentId;
    }

    [TestMethod]
    public async Task CreateIncident_ValidInput_ReturnsNumber()
    {
        var result = await Actions.CreateIncident(new CreateIncidentRequest
        {
            ShortDescription = "Blackbird integration test incident (create)"
        });
        Console.WriteLine($"Created {result.Number} ({result.IncidentId}) state={result.State}");
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Number));

        await Actions.DeleteIncident(new IncidentIdentifier { IncidentId = result.IncidentId });
    }

    [TestMethod]
    public async Task GetIncident_ValidId_ReturnsRecord()
    {
        var id = await CreateDisposableIncidentAsync();
        var result = await Actions.GetIncident(new IncidentIdentifier { IncidentId = id });
        Console.WriteLine($"{result.Number}: {result.ShortDescription} state={result.State}");
        Assert.AreEqual(id, result.IncidentId);

        await Actions.DeleteIncident(new IncidentIdentifier { IncidentId = id });
    }

    [TestMethod]
    public async Task UpdateIncident_ValidInput_ReflectsChange()
    {
        var id = await CreateDisposableIncidentAsync();
        var result = await Actions.UpdateIncident(new UpdateIncidentRequest
        {
            IncidentId = id,
            State = "2",
            ShortDescription = "Blackbird integration test incident (updated)"
        });
        Console.WriteLine($"Updated state={result.State}");
        Assert.AreEqual("2", result.State);

        await Actions.DeleteIncident(new IncidentIdentifier { IncidentId = id });
    }

    [TestMethod]
    public async Task SearchIncidents_ReturnsResults()
    {
        var result = await Actions.SearchIncidents(new SearchIncidentsRequest { Limit = 5 });
        Console.WriteLine($"Found {result.TotalCount} incidents");
        Assert.IsNotNull(result.Incidents);
    }

    [TestMethod]
    public async Task SearchIncidents_NoMatch_ReturnsEmpty()
    {
        var result = await Actions.SearchIncidents(new SearchIncidentsRequest
        {
            Query = "short_descriptionLIKEzzz_nonexistent_zzz"
        });
        Console.WriteLine($"Found {result.TotalCount} incidents");
        Assert.IsNotNull(result.Incidents);
        Assert.AreEqual(0, result.Incidents.Count);
    }

    [TestMethod]
    public async Task AddCommentAndGetComments_Roundtrip()
    {
        var id = await CreateDisposableIncidentAsync("Blackbird comment test incident");

        await Actions.AddCommentToIncident(new AddCommentRequest { IncidentId = id, Comment = "First test comment" });
        await Actions.AddCommentToIncident(new AddCommentRequest { IncidentId = id, Comment = "Second test comment" });

        var comments = await Actions.GetIncidentComments(new IncidentIdentifier { IncidentId = id });
        Console.WriteLine($"Found {comments.Comments.Count} comments");
        foreach (var c in comments.Comments)
            Console.WriteLine($"{c.CreatedAt}: {c.Value} ({c.CreatedBy})");

        Assert.IsTrue(comments.Comments.Count >= 2);

        await Actions.DeleteIncident(new IncidentIdentifier { IncidentId = id });
    }

    [TestMethod]
    public async Task DeleteIncident_ValidId_ReturnsSuccess()
    {
        var id = await CreateDisposableIncidentAsync("Blackbird delete test incident");
        var result = await Actions.DeleteIncident(new IncidentIdentifier { IncidentId = id });
        Assert.IsTrue(result.Success);
    }
}
