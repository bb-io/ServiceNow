namespace Apps.ServiceNow.Models.Content;

/// <summary>
/// Everything the converter needs to emit one self-describing translatable HTML file for a
/// ServiceNow knowledge article. Built from the raw (writable) kb_knowledge record so the file
/// can later be reversed back onto the same fields.
/// </summary>
public class ArticleHtmlModel
{
    /// <summary>The article sys_id — becomes the entry id, the UCID and the blackbird-key prefix.</summary>
    public string EntryId { get; set; } = string.Empty;

    /// <summary>The language variant of this file (html lang + blackbird-locale).</summary>
    public string Locale { get; set; } = "en";

    /// <summary>short_description — the translatable title (plain text).</summary>
    public string? Title { get; set; }

    /// <summary>text — the translatable body (HTML markup).</summary>
    public string? Body { get; set; }

    public string? AdminUrl { get; set; }
    public string? PublicUrl { get; set; }
    public string? SystemRef { get; set; }

    /// <summary>Optional "updated by" display name, stamped as ITS review provenance.</summary>
    public string? UpdatedByName { get; set; }
}
