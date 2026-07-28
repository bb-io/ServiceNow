using System.Net;
using Apps.ServiceNow.Actions;
using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Identifiers;
using Apps.ServiceNow.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using HtmlAgilityPack;
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
        var result = await Actions.SearchArticles(new SearchArticlesRequest { Limit = 5, CreatedAfter = DateTime.Today.AddDays(-1) });
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
        Assert.AreEqual(KnownArticleId, result.ContentId);
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

    [TestMethod]
    public async Task SearchArticles_ReturnsCreatedAndUpdatedTimestamps()
    {
        var result = await Actions.SearchArticles(new SearchArticlesRequest { Limit = 5 });

        Assert.IsTrue(result.Articles.Count > 0);
        foreach (var a in result.Articles)
            Console.WriteLine($"{a.Number} [{a.State}/{a.Language}] created {a.CreatedOn:u} updated {a.UpdatedOn:u}");

        Assert.IsTrue(result.Articles.All(a => a.CreatedOn.HasValue), "Every hit should carry its creation date.");
        Assert.IsTrue(result.Articles.All(a => a.UpdatedOn.HasValue), "Every hit should carry its update date.");
    }

    [TestMethod]
    public async Task SearchArticles_CreatedAfter_ExcludesOlderArticles()
    {
        var cutoff = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var all = await Actions.SearchArticles(new SearchArticlesRequest());
        var filtered = await Actions.SearchArticles(new SearchArticlesRequest { CreatedAfter = cutoff });

        Console.WriteLine($"{all.TotalCount} articles total, {filtered.TotalCount} created after {cutoff:u}");

        Assert.IsTrue(filtered.TotalCount > 0, "The instance should hold articles created after the cutoff.");
        Assert.IsTrue(filtered.TotalCount < all.TotalCount, "The date filter has to narrow the result set.");
        Assert.IsTrue(filtered.Articles.All(a => a.CreatedOn >= cutoff),
            "No article created before the cutoff may be returned.");
    }

    [TestMethod]
    public async Task SearchArticles_CreatedBefore_ExcludesNewerArticles()
    {
        var cutoff = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var filtered = await Actions.SearchArticles(new SearchArticlesRequest { CreatedBefore = cutoff });

        Console.WriteLine($"{filtered.TotalCount} articles created before {cutoff:u}");
        Assert.IsTrue(filtered.TotalCount > 0);
        Assert.IsTrue(filtered.Articles.All(a => a.CreatedOn <= cutoff),
            "No article created after the cutoff may be returned.");
    }

    [TestMethod]
    public async Task SearchArticles_DraftState_ReturnsUnpublishedArticles()
    {
        var result = await Actions.SearchArticles(new SearchArticlesRequest { State = "draft", Limit = 10 });

        Console.WriteLine($"Found {result.TotalCount} draft articles");
        Assert.IsTrue(result.Articles.Count > 0,
            "Drafts must be searchable; the KM search endpoint only ever returned published articles.");
        Assert.IsTrue(result.Articles.All(a => a.State == "draft"));
    }

    [TestMethod]
    public async Task UploadArticle_OtherLanguage_WritesToVariantAndLeavesSourceIntact()
    {
        const string germanTitle = "Blackbird Variantentest (DE)";
        const string germanBody = "<p>Dies ist der <strong>deutsche</strong> Inhalt der Variante.</p>";

        var source = await Actions.CreateArticle(new CreateArticleRequest
        {
            Language = "en",
            Title = "Blackbird variant test article",
            KnowledgeBaseId = KnowledgeBaseId,
            Content = "<p>This is the <strong>English</strong> source content.</p>"
        });

        var downloaded = await Actions.DownloadArticle(new DownloadArticleRequest
        {
            ContentId = source.ArticleId,
            Locale = "en"
        });
        var translated = TranslateDownloadedFile(downloaded.Content!, germanTitle, germanBody);

        var upload = await Actions.UploadArticle(new UploadArticleRequest
        {
            Content = translated,
            Locale = "de",
            ContentId = source.ArticleId
        });

        Console.WriteLine($"root={upload.RootEntryId} target={upload.TargetEntryId}");
        Assert.IsNull(upload.Errors, "The upload should not report per-article errors.");
        Assert.AreNotEqual(source.ArticleId, upload.TargetEntryId,
            "A German translation must land on its own article, not on the English source.");

        var sourceAfter = await Actions.GetArticleMetadata(new ArticleIdentifier { ArticleId = source.ArticleId });
        Assert.AreEqual("en", sourceAfter.Language, "The source article must keep its language.");
        Assert.AreEqual("Blackbird variant test article", sourceAfter.Title,
            "The source article's title must not be overwritten by the translation.");

        var variant = await Actions.GetArticleMetadata(new ArticleIdentifier { ArticleId = upload.TargetEntryId });
        Assert.AreEqual("de", variant.Language, "The written article must be the German variant.");
        Assert.AreEqual(germanTitle, variant.Title);

        // Uploading the same language a second time must reuse the variant rather than create another one.
        var second = await Actions.UploadArticle(new UploadArticleRequest
        {
            Content = translated,
            Locale = "de",
            ContentId = source.ArticleId
        });
        Assert.AreEqual(upload.TargetEntryId, second.TargetEntryId,
            "A second upload of the same language has to reuse the existing variant.");
    }

    [TestMethod]
    public async Task UploadArticle_InactiveLanguage_ThrowsMisconfiguration()
    {
        var downloaded = await Actions.DownloadArticle(new DownloadArticleRequest
        {
            ContentId = KnownArticleId,
            Locale = "en"
        });
        var file = TranslateDownloadedFile(downloaded.Content!, "titre", "<p>corps</p>");

        var ex = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() =>
            Actions.UploadArticle(new UploadArticleRequest
            {
                Content = file,
                Locale = "fr",
                ContentId = KnownArticleId
            }));

        Console.WriteLine(ex.Message);
        StringAssert.Contains(ex.Message, "not an active language");
    }

    /// <summary>
    /// Stands in for the external translation step: rewrites the downloaded file's field values and places it where
    /// the test file manager reads uploads from.
    /// </summary>
    private FileReference TranslateDownloadedFile(FileReference downloaded, string title, string body)
    {
        var projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
        var html = File.ReadAllText(Path.Combine(projectDirectory, "TestFiles", "Output", downloaded.Name));

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        foreach (var node in doc.DocumentNode.SelectNodes($"//*[@{RoundtripHtml.FieldIdAttr}]"))
        {
            var fieldId = node.GetAttributeValue(RoundtripHtml.FieldIdAttr, string.Empty);
            if (fieldId == RoundtripHtml.TitleFieldId)
                node.InnerHtml = WebUtility.HtmlEncode(title);
            else if (fieldId == RoundtripHtml.BodyFieldId)
                node.InnerHtml = body;
        }

        var name = $"translated_{downloaded.Name}";
        File.WriteAllText(Path.Combine(projectDirectory, "TestFiles", "Input", name), doc.DocumentNode.OuterHtml);

        return new FileReference { Name = name, ContentType = "text/html" };
    }
}
