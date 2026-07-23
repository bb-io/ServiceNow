using Apps.ServiceNow.Actions;
using Apps.ServiceNow.Models.Identifiers;
using Apps.ServiceNow.Models.Requests;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.ServiceNow.Base;

namespace Tests.ServiceNow;

[TestClass]
public class ArticleActionTests : TestBase
{
    private const string KnownArticleId = "207de43187032100deddb882a2e3ec7a";
    private const string KnowledgeBaseId = "dfc19531bf2021003f07e2c1ac0739ab";

    private ArticleActions Actions => new(InvocationContext, FileManager);

    [TestMethod]
    public async Task SearchArticles_NoFilters_ReturnsResults()
    {
        var result = await Actions.SearchArticles(new SearchArticlesRequest { Limit = 5 });
        Console.WriteLine($"Found {result.TotalCount} articles");
        foreach (var a in result.Articles)
            Console.WriteLine($"{a.Number}: {a.Title} ({a.ArticleId})");

        Assert.IsNotNull(result.Articles);
        Assert.IsTrue(result.Articles.Count > 0);
        Assert.IsFalse(result.Articles.Any(a => a.ArticleId.Contains(':')), "Article IDs should have the kb_knowledge: prefix stripped.");
    }

    [TestMethod]
    public async Task SearchArticles_TextQuery_ReturnsRankedResults()
    {
        var result = await Actions.SearchArticles(new SearchArticlesRequest { Query = "email", Limit = 5 });
        Console.WriteLine($"Found {result.TotalCount} articles for 'email'");
        Assert.IsTrue(result.Articles.Count > 0);
    }

    [TestMethod]
    public async Task GetArticleMetadata_ValidId_ReturnsFields()
    {
        var result = await Actions.GetArticleMetadata(new ArticleIdentifier { ArticleId = KnownArticleId });
        Console.WriteLine($"{result.Number}: {result.Title} [{result.State}]");
        Assert.AreEqual(KnownArticleId, result.ArticleId);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Number));
    }

    [TestMethod]
    public async Task DownloadArticle_ValidId_ReturnsHtmlFile()
    {
        var result = await Actions.DownloadArticle(new DownloadArticleRequest
        {
            ContentId = KnownArticleId,
            Locale = "en"
        });
        Console.WriteLine($"Downloaded {result.Content?.Name}, root {result.RootEntryId}");
        Assert.IsNotNull(result.Content);
        Assert.AreEqual(KnownArticleId, result.RootEntryId);
    }

    [TestMethod]
    public async Task CreateArticle_ValidInput_ReturnsId()
    {
        var result = await Actions.CreateArticle(new CreateArticleRequest
        {
            Language = "en",
            Title = "Blackbird integration test article",
            KnowledgeBaseId = KnowledgeBaseId,
            Content = "<p>Created by an integration test.</p>"
        });
        Console.WriteLine($"Created {result.Number} ({result.ArticleId}) state={result.State}");
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.ArticleId));
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Number));
    }

    [TestMethod]
    public async Task UpdateArticleMetadata_ValidInput_ReflectsChange()
    {
        var created = await Actions.CreateArticle(new CreateArticleRequest
        {
            Language = "en",
            Title = "Blackbird update test article",
            KnowledgeBaseId = KnowledgeBaseId
        });

        var newTitle = "Blackbird update test article (updated)";
        var result = await Actions.UpdateArticleMetadata(new UpdateArticleRequest
        {
            ArticleId = created.ArticleId,
            Title = newTitle
        });
        Console.WriteLine($"Updated title: {result.Title}");
        Assert.AreEqual(newTitle, result.Title);
    }

    [TestMethod]
    public async Task Roundtrip_DownloadThenUpload_WritesTranslatedFieldsBack()
    {
        var downloaded = await Actions.DownloadArticle(new DownloadArticleRequest
        {
            ContentId = KnownArticleId,
            Locale = "en"
        });
        Assert.IsNotNull(downloaded.Content);

        var result = await Actions.UploadArticle(new UploadArticleRequest
        {
            Content = downloaded.Content,
            Locale = "en",
            ContentId = KnownArticleId
        });
        Console.WriteLine($"Upload root {result.RootEntryId}, errors {result.Errors?.Count ?? 0}");
        Assert.IsNotNull(result.Content);
        Assert.IsNull(result.Errors, "An unedited roundtrip should not produce per-article errors.");
    }
}
