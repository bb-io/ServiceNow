using System.Text;
using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Content;
using Apps.ServiceNow.Utils;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using HtmlAgilityPack;

namespace Tests.ServiceNow;

/// <summary>
/// Pure, API-free tests of the translation roundtrip converters (record -> HTML and HTML -> fields).
/// They exercise the self-describing-file contract from simple to complex without any network access.
/// </summary>
[TestClass]
public class ConverterTests
{
    private const string ArticleId = "207de43187032100deddb882a2e3ec7a";

    private static ArticleHtmlModel SampleModel(string? title, string? body, string locale = "en") => new()
    {
        EntryId = ArticleId,
        Locale = locale,
        Title = title,
        Body = body,
        AdminUrl = $"https://dev190007.service-now.com/kb_knowledge.do?sys_id={ArticleId}",
        PublicUrl = "https://dev190007.service-now.com/kb_view.do?sysparm_article=KB0000024",
        SystemRef = "https://dev190007.service-now.com"
    };

    private static HtmlDocument Load(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }

    // -----------------------------------------------------------------------------------------
    // Metadata / skeleton
    // -----------------------------------------------------------------------------------------

    [TestMethod]
    public void ToHtml_EmitsRequiredMetadataAndSkeleton()
    {
        var html = ArticleHtmlConverter.ToHtml(SampleModel("Create An Email Signature", "<p>Hello</p>"));
        var doc = Load(html);

        Assert.AreEqual("en", doc.DocumentNode.SelectSingleNode("//html").GetAttributeValue("lang", ""));
        Assert.AreEqual(ArticleId, Meta(doc, "ucid"));
        Assert.IsNull(Meta(doc, "entry-id"), "blackbird-entry-id is redundant with blackbird-ucid and must not be emitted.");
        Assert.AreEqual("en", Meta(doc, "locale"));
        Assert.AreEqual("Create An Email Signature", Meta(doc, "content-name"));
        Assert.AreEqual("ServiceNow", Meta(doc, "system-name"));
        Assert.AreEqual("https://dev190007.service-now.com", Meta(doc, "system-ref"));

        var body = doc.DocumentNode.SelectSingleNode("//body");
        Assert.AreEqual("ServiceNow", body.GetAttributeValue("its-rev-tool", ""));

        var entry = doc.DocumentNode.SelectSingleNode($"//*[@{RoundtripHtml.EntryIdAttr}]");
        Assert.IsNotNull(entry, "There must be one object container.");
        Assert.AreEqual(ArticleId, entry.GetAttributeValue(RoundtripHtml.EntryIdAttr, ""));
    }

    [TestMethod]
    public void ToHtml_TitleFieldCarriesReconstructionAttributes()
    {
        var html = ArticleHtmlConverter.ToHtml(SampleModel("My title", "<p>Body</p>"));
        var doc = Load(html);

        var title = doc.DocumentNode.SelectSingleNode($"//*[@{RoundtripHtml.FieldIdAttr}='short_description']");
        Assert.IsNotNull(title);
        Assert.AreEqual(RoundtripHtml.StringType, title.GetAttributeValue(RoundtripHtml.FieldTypeAttr, ""));
        Assert.AreEqual($"{ArticleId}-short_description", title.GetAttributeValue(RoundtripHtml.BlackbirdKeyAttr, ""));

        var size = title.GetAttributeValue(RoundtripHtml.BlackbirdSizeAttr, "");
        StringAssert.Contains(HtmlEntity.DeEntitize(size), "\"MaximumSize\":160");
    }

    [TestMethod]
    public void ToHtml_BodyFieldIsMarkedAsHtml()
    {
        var html = ArticleHtmlConverter.ToHtml(SampleModel("T", "<p>Body</p>"));
        var doc = Load(html);

        var bodyField = doc.DocumentNode.SelectSingleNode($"//*[@{RoundtripHtml.FieldIdAttr}='text']");
        Assert.IsNotNull(bodyField);
        Assert.AreEqual(RoundtripHtml.HtmlType, bodyField.GetAttributeValue(RoundtripHtml.FieldTypeAttr, ""));
        Assert.AreEqual("true", bodyField.GetAttributeValue(RoundtripHtml.HtmlAttr, ""));
    }

    [TestMethod]
    public void ToHtml_SkipsEmptyFields()
    {
        var html = ArticleHtmlConverter.ToHtml(SampleModel(title: null, body: "<p>Only body</p>"));
        var doc = Load(html);

        Assert.IsNull(doc.DocumentNode.SelectSingleNode($"//*[@{RoundtripHtml.FieldIdAttr}='short_description']"),
            "A null title must not produce a title field node.");
        Assert.IsNotNull(doc.DocumentNode.SelectSingleNode($"//*[@{RoundtripHtml.FieldIdAttr}='text']"));
    }

    // -----------------------------------------------------------------------------------------
    // Round-trip: simple -> complex
    // -----------------------------------------------------------------------------------------

    [TestMethod]
    public void Roundtrip_SimpleTitleAndBody()
    {
        AssertRoundtrip("A simple title", "<p>A simple paragraph.</p>");
    }

    [TestMethod]
    public void Roundtrip_TitleWithSpecialCharacters()
    {
        AssertRoundtrip("Tom & Jerry: <b>the</b> \"best\" duo", "<p>Fish &amp; chips cost &lt; £5.</p>");
    }

    [TestMethod]
    public void Roundtrip_BodyWithNestedMarkup()
    {
        var body =
            "<h2>Steps</h2>" +
            "<ol><li>Open <strong>Settings</strong></li><li>Choose <em>Signature</em></li></ol>" +
            "<p>See <a href=\"https://example.com\">the guide</a>.</p>";
        AssertRoundtrip("How to create a signature", body);
    }

    [TestMethod]
    public void Roundtrip_ComplexBodyWithTablesAndAttributes()
    {
        var body =
            "<p><strong><span style=\"font-size: 18pt;\">Create An Email Signature</span></strong></p>\n" +
            "<p>To create a personalized email signature:</p>\n" +
            "<table border=\"1\"><tbody><tr><td>Field</td><td>Value</td></tr>" +
            "<tr><td>Name</td><td>Jane</td></tr></tbody></table>\n" +
            "<ul><li>Item one</li><li>Item two</li></ul>" +
            "<img src=\"https://dev190007.service-now.com/logo.png\" alt=\"Logo\" />";
        AssertRoundtrip("Create An Email Signature", body);
    }

    [TestMethod]
    public void Roundtrip_NonDefaultLocaleIsPreserved()
    {
        var html = ArticleHtmlConverter.ToHtml(SampleModel("Titre", "<p>Corps</p>", locale: "fr"));
        var parsed = ArticleHtmlConverter.ParseHtml(html);

        Assert.AreEqual("fr", parsed.MainLocale);
        Assert.AreEqual(ArticleId, parsed.MainEntryId);
    }

    // -----------------------------------------------------------------------------------------
    // Parsing behaviour
    // -----------------------------------------------------------------------------------------

    [TestMethod]
    public void ParseHtml_ReturnsEntryWithBothFields()
    {
        var html = ArticleHtmlConverter.ToHtml(SampleModel("Title", "<p>Body</p>"));
        var parsed = ArticleHtmlConverter.ParseHtml(html);

        Assert.AreEqual(1, parsed.Entries.Count);
        var entry = parsed.Entries[0];
        Assert.AreEqual(ArticleId, entry.EntryId);
        Assert.AreEqual(2, entry.Fields.Count);

        var title = entry.Fields.Single(f => f.FieldId == "short_description");
        Assert.IsFalse(title.IsHtml);
        Assert.AreEqual("Title", title.Value);

        var body = entry.Fields.Single(f => f.FieldId == "text");
        Assert.IsTrue(body.IsHtml);
        StringAssert.Contains(body.Value, "<p>Body</p>");
    }

    [TestMethod]
    public void ParseHtml_ReadsTranslatorEditedValues()
    {
        // Simulate a translator who replaced the visible text but kept the data-* attributes.
        var html = ArticleHtmlConverter.ToHtml(SampleModel("Original title", "<p>Original body</p>"));
        var translated = html
            .Replace("Original title", "Titre traduit")
            .Replace("Original body", "Corps traduit");

        var parsed = ArticleHtmlConverter.ParseHtml(translated);
        var entry = parsed.Entries.Single();

        Assert.AreEqual("Titre traduit", entry.Fields.Single(f => f.FieldId == "short_description").Value);
        StringAssert.Contains(entry.Fields.Single(f => f.FieldId == "text").Value, "Corps traduit");
    }

    [TestMethod]
    public void ParseHtml_IgnoresNodesWithoutFieldAttributes()
    {
        // A plain paragraph the translator might add outside a field node must not become a field.
        var html = ArticleHtmlConverter.ToHtml(SampleModel("Title", "<p>Body</p>"));
        var withNoise = html.Replace("</body>", "<div><p>Not a field</p></div></body>");

        var parsed = ArticleHtmlConverter.ParseHtml(withNoise);
        Assert.AreEqual(2, parsed.Entries.Single().Fields.Count);
    }

    [TestMethod]
    public void NormalizeHtml_TreatsEquivalentMarkupAsEqual()
    {
        // Re-serialized body from a roundtrip should normalize equal to the original stored body.
        var original = "<p>Hello <strong>world</strong></p>";
        var html = ArticleHtmlConverter.ToHtml(SampleModel("T", original));
        var parsedBody = ArticleHtmlConverter.ParseHtml(html).Entries.Single()
            .Fields.Single(f => f.FieldId == "text").Value;

        Assert.AreEqual(
            ArticleHtmlConverter.NormalizeHtml(original),
            ArticleHtmlConverter.NormalizeHtml(parsedBody));
    }

    // -----------------------------------------------------------------------------------------
    // Blackbird.Filters interoperability (mirrors what the Upload action does with the file)
    // -----------------------------------------------------------------------------------------

    [TestMethod]
    public void Transformation_LoadThenTargetThenParse_PreservesFieldStructure()
    {
        var html = ArticleHtmlConverter.ToHtml(
            SampleModel("Create An Email Signature", "<p>Body <strong>text</strong></p>"));
        var bytes = Encoding.UTF8.GetBytes(html);

        var load = Transformation.Load(new MemoryStream(bytes), "article_en.html", "text/html");
        Assert.IsTrue(load.Success, "The generated file must be Blackbird-interoperable.");

        var target = load.Target();
        Assert.IsTrue(target.Success);

        var targetHtml = target.Value.ToStream(MetadataHandling.Include).ReadString();
        var parsed = ArticleHtmlConverter.ParseHtml(targetHtml);

        Assert.AreEqual(1, parsed.Entries.Count, "The entry container must survive the wrapper.");
        var entry = parsed.Entries.Single();
        Assert.IsTrue(entry.Fields.Any(f => f.FieldId == "short_description"),
            "The title field must survive the wrapper.");
        Assert.IsTrue(entry.Fields.Any(f => f.FieldId == "text"),
            "The body field must survive the wrapper.");
    }

    [TestMethod]
    public void Transformation_CarriesTranslatedText_ThroughTarget()
    {
        // A translator edits the visible text; after the wrapper roundtrip the new text must be readable.
        var html = ArticleHtmlConverter.ToHtml(SampleModel("Source title", "<p>Source body</p>"));
        var translated = html.Replace("Source title", "Target title").Replace("Source body", "Target body");
        var bytes = Encoding.UTF8.GetBytes(translated);

        var load = Transformation.Load(new MemoryStream(bytes), "article_en.html", "text/html");
        var target = load.Target();
        var targetHtml = target.Success
            ? target.Value.ToStream(MetadataHandling.Include).ReadString()
            : translated;

        var entry = ArticleHtmlConverter.ParseHtml(targetHtml).Entries.Single();
        Assert.AreEqual("Target title", entry.Fields.Single(f => f.FieldId == "short_description").Value);
        StringAssert.Contains(entry.Fields.Single(f => f.FieldId == "text").Value, "Target body");
    }

    // -----------------------------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------------------------

    private static void AssertRoundtrip(string title, string body)
    {
        var html = ArticleHtmlConverter.ToHtml(SampleModel(title, body));

        // The produced document must itself be parseable HTML.
        var parsed = ArticleHtmlConverter.ParseHtml(html);
        var entry = parsed.Entries.Single();

        Assert.AreEqual(title, entry.Fields.Single(f => f.FieldId == "short_description").Value,
            "Title must survive the roundtrip unchanged.");
        Assert.AreEqual(
            ArticleHtmlConverter.NormalizeHtml(body),
            ArticleHtmlConverter.NormalizeHtml(entry.Fields.Single(f => f.FieldId == "text").Value),
            "Body markup must survive the roundtrip unchanged.");

        Console.WriteLine(html);
    }

    private static string? Meta(HtmlDocument doc, string name) =>
        doc.DocumentNode.SelectSingleNode($"//meta[@name='blackbird-{name}']")?.GetAttributeValue("content", "");
}
