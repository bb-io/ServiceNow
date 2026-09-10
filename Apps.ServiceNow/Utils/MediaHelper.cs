using System.Text.RegularExpressions;
using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Extensions;
using HtmlAgilityPack;

namespace Apps.ServiceNow.Utils;

public static class MediaHelper
{
    private static readonly (string Xpath, string Attribute)[] UrlAttributes =
    [
        ("//img", "src"),
        ("//source", "src"),
        ("//a", "href")
    ];
    
    private static readonly Regex SysIdPattern = new("[0-9a-f]{32}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static async Task<string?> InlineImages(
        string? html,
        Uri instanceBaseUrl,
        Func<string, Task<(byte[] Bytes, string ContentType)>> fetchAttachment)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes("//img[@src]");
        if (nodes is null)
            return html;

        string origin = instanceBaseUrl.GetLeftPart(UriPartial.Authority);
        bool changed = false;

        foreach (var node in nodes)
        {
            string src = node.GetAttributeValue("src", string.Empty);
            if (!src.StartsWith(origin, StringComparison.OrdinalIgnoreCase))
                continue;

            var match = SysIdPattern.Match(src);
            if (!match.Success)
                continue;

            try
            {
                var (bytes, contentType) = await fetchAttachment(match.Value);
                if (bytes.Length == 0)
                    continue;

                node.SetAttributeValue(RoundtripHtml.AttachmentIdAttr, match.Value);
                node.SetAttributeValue("src", $"data:{contentType};base64,{Convert.ToBase64String(bytes)}");
                changed = true;
            }
            catch
            {
                // ignored - one unreadable image won't fail the whole download
            }
        }

        return changed ? doc.DocumentNode.OuterHtml : html;
    }

    public static string? RestoreImageUrls(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes($"//img[@{RoundtripHtml.AttachmentIdAttr}]");
        if (nodes is null)
            return html;

        foreach (var node in nodes)
        {
            node.SetAttributeValue("src", $"/{node.GetAttributeValue(RoundtripHtml.AttachmentIdAttr, string.Empty)}.iix");
            node.Attributes.Remove(RoundtripHtml.AttachmentIdAttr);
        }

        return doc.DocumentNode.OuterHtml;
    }

    public static bool ContainsInlinedImages(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return false;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return doc.DocumentNode
            .SelectNodes("//img[@src]")?.Any(n => n.GetAttributeValue("src", string.Empty)
            .StartsWith("data:", StringComparison.OrdinalIgnoreCase)) == true;
    }

    public static string? ToAbsoluteUrls(string? html, Uri instanceBaseUrl)
    {
        return Rewrite(html, value => value.ToAbsoluteUrlString(instanceBaseUrl));
    }

    public static string? ToRelativeUrls(string? html, Uri instanceBaseUrl)
    {
        return Rewrite(html, value => value.ToRelativeUrlString(instanceBaseUrl));
    }

    private static string? Rewrite(string? html, Func<string, string?> transform)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        bool changed = false;
        foreach (var (xpath, attribute) in UrlAttributes)
        {
            var nodes = doc.DocumentNode.SelectNodes($"{xpath}[@{attribute}]");
            if (nodes is null) 
                continue;

            foreach (var node in nodes)
            {
                string current = node.GetAttributeValue(attribute, string.Empty);
                string? rewritten = transform(current);
                if (rewritten is null || rewritten == current) 
                    continue;

                node.SetAttributeValue(attribute, rewritten);
                changed = true;
            }
        }

        return changed ? doc.DocumentNode.OuterHtml : html;
    }
}