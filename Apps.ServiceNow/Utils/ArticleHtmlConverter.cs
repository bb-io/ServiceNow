using System.Net;
using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models;
using Apps.ServiceNow.Models.Content;
using Blackbird.Filters.Extensions;
using HtmlAgilityPack;

namespace Apps.ServiceNow.Utils;

/// <summary>
/// The two halves of the translation roundtrip contract for ServiceNow knowledge articles:
/// <see cref="ToHtml"/> turns a kb_knowledge record into a self-describing HTML file, and
/// <see cref="ParseHtml"/> reads that file (or a translated copy of it) back into fields.
/// Both are pure and API-free so they can be unit-tested offline.
/// </summary>
public static class ArticleHtmlConverter
{
    // -----------------------------------------------------------------------------------------
    // JSON/record -> HTML
    // -----------------------------------------------------------------------------------------

    public static string ToHtml(ArticleHtmlModel model)
    {
        var doc = new HtmlDocument();

        var html = doc.CreateElement("html");
        html.SetAttributeValue("lang", model.Locale);
        doc.DocumentNode.AppendChild(html);

        var head = doc.CreateElement("head");
        html.AppendChild(head);
        AddMeta(doc, head, RoundtripHtml.MetaLocale, model.Locale);
        AddMeta(doc, head, RoundtripHtml.MetaUcid, model.EntryId);
        AddMeta(doc, head, RoundtripHtml.MetaContentName, model.Title ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(model.AdminUrl))
            AddMeta(doc, head, RoundtripHtml.MetaAdminUrl, model.AdminUrl!);
        if (!string.IsNullOrWhiteSpace(model.PublicUrl))
            AddMeta(doc, head, RoundtripHtml.MetaPublicUrl, model.PublicUrl!);
        AddMeta(doc, head, RoundtripHtml.MetaSystemName, RoundtripHtml.SystemName);
        if (!string.IsNullOrWhiteSpace(model.SystemRef))
            AddMeta(doc, head, RoundtripHtml.MetaSystemRef, model.SystemRef!);

        var body = doc.CreateElement("body");
        html.AppendChild(body);
        body.SetAttributeValue("its-rev-tool", RoundtripHtml.SystemName);
        if (!string.IsNullOrWhiteSpace(model.SystemRef))
            body.SetAttributeValue("its-rev-tool-ref", model.SystemRef);
        if (!string.IsNullOrWhiteSpace(model.UpdatedByName))
            body.SetAttributeValue("its-rev-person", model.UpdatedByName);

        var entry = doc.CreateElement("div");
        entry.SetAttributeValue(RoundtripHtml.EntryIdAttr, model.EntryId);
        body.AppendChild(entry);

        // Title (short_description) — plain text in an <h1>. Structural characters are HTML-encoded
        // so a title containing <, & or " cannot corrupt the document and round-trips exactly.
        if (!string.IsNullOrEmpty(model.Title))
        {
            var titleNode = doc.CreateElement("h1");
            titleNode.InnerHtml = WebUtility.HtmlEncode(model.Title);
            ApplyFieldAttributes(titleNode, model.EntryId, RoundtripHtml.TitleFieldId,
                RoundtripHtml.StringType, isHtml: false,
                new SizeRestrictions { MaximumSize = RoundtripHtml.TitleMaxLength });
            entry.AppendChild(titleNode);
        }

        // Body (text) — HTML markup in a <div>.
        if (!string.IsNullOrEmpty(model.Body))
        {
            var bodyNode = doc.CreateElement("div");
            bodyNode.InnerHtml = model.Body;
            ApplyFieldAttributes(bodyNode, model.EntryId, RoundtripHtml.BodyFieldId,
                RoundtripHtml.HtmlType, isHtml: true, size: null);
            entry.AppendChild(bodyNode);
        }

        return doc.DocumentNode.OuterHtml;
    }

    // -----------------------------------------------------------------------------------------
    // HTML -> JSON/fields
    // -----------------------------------------------------------------------------------------

    public static ParsedArticleFile ParseHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html ?? string.Empty);

        var result = new ParsedArticleFile
        {
            MainEntryId = ReadMeta(doc, RoundtripHtml.MetaUcid) ?? ReadMeta(doc, RoundtripHtml.MetaEntryId),
            MainLocale = ReadMeta(doc, RoundtripHtml.MetaLocale)
                         ?? doc.DocumentNode.SelectSingleNode("//html")?.GetAttributeValue("lang", string.Empty).NullIfEmpty()
        };

        var entryNodes = doc.DocumentNode.SelectNodes($"//*[@{RoundtripHtml.EntryIdAttr}]");
        if (entryNodes is null)
            return result;

        foreach (var entryNode in entryNodes)
        {
            var entry = new ParsedEntry
            {
                EntryId = entryNode.GetAttributeValue(RoundtripHtml.EntryIdAttr, string.Empty)
            };

            var fieldNodes = entryNode.SelectNodes(
                $".//*[@{RoundtripHtml.FieldIdAttr} and @{RoundtripHtml.FieldTypeAttr}]");
            if (fieldNodes is not null)
            {
                foreach (var fieldNode in fieldNodes)
                {
                    var fieldType = fieldNode.GetAttributeValue(RoundtripHtml.FieldTypeAttr, string.Empty);
                    var isHtml = fieldNode.GetAttributeValue(RoundtripHtml.HtmlAttr, "false") == "true"
                                 || fieldType == RoundtripHtml.HtmlType;

                    var value = isHtml
                        ? (fieldNode.InnerHtml ?? string.Empty).Trim()
                        : (WebUtility.HtmlDecode(fieldNode.InnerText ?? string.Empty) ?? string.Empty).Trim();

                    entry.Fields.Add(new ParsedField
                    {
                        FieldId = fieldNode.GetAttributeValue(RoundtripHtml.FieldIdAttr, string.Empty),
                        FieldType = fieldType,
                        IsHtml = isHtml,
                        Value = value
                    });
                }
            }

            result.Entries.Add(entry);
        }

        return result;
    }

    /// <summary>
    /// Normalizes an HTML fragment (parse + re-serialize) so two semantically equal bodies that
    /// differ only in incidental formatting compare equal — used to skip no-op API writes.
    /// </summary>
    public static string NormalizeHtml(string? html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc.DocumentNode.OuterHtml.Trim();
    }

    // -----------------------------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------------------------

    private static void ApplyFieldAttributes(HtmlNode node, string entryId, string fieldId,
        string fieldType, bool isHtml, SizeRestrictions? size)
    {
        node.SetAttributeValue(RoundtripHtml.FieldTypeAttr, fieldType);
        node.SetAttributeValue(RoundtripHtml.FieldIdAttr, fieldId);
        node.SetAttributeValue(RoundtripHtml.BlackbirdKeyAttr, $"{entryId}-{fieldId}");
        if (isHtml)
            node.SetAttributeValue(RoundtripHtml.HtmlAttr, "true");

        var serialized = SizeRestrictionHelper.Serialize(size);
        if (serialized is not null)
            node.SetAttributeValue(RoundtripHtml.BlackbirdSizeAttr, serialized);
    }

    private static void AddMeta(HtmlDocument doc, HtmlNode head, string name, string value)
    {
        var meta = doc.CreateElement("meta");
        meta.SetAttributeValue("name", $"blackbird-{name}");
        meta.SetAttributeValue("content", value ?? string.Empty);
        head.AppendChild(meta);
    }

    private static string? ReadMeta(HtmlDocument doc, string name)
    {
        var node = doc.DocumentNode.SelectSingleNode($"//meta[@name='blackbird-{name}']");
        var value = node?.GetAttributeValue("content", string.Empty);
        return string.IsNullOrEmpty(value) ? null : HtmlEntity.DeEntitize(value);
    }
}
