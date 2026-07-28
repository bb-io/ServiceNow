using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Apps.ServiceNow.Utils;

/// <summary>
/// Builds the short plain-text preview that 'Search articles' returns per hit. The table API hands back the
/// raw HTML body, so we strip the markup ourselves instead of relying on the KM API's snippet.
/// </summary>
public static class HtmlPreview
{
    private const int MaxLength = 240;

    public static string Build(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var text = WebUtility.HtmlDecode(doc.DocumentNode.InnerText ?? string.Empty);
        text = Regex.Replace(text, @"\s+", " ").Trim();

        return text.Length <= MaxLength ? text : text[..MaxLength].TrimEnd() + "…";
    }
}
