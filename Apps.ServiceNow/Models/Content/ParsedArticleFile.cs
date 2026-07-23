namespace Apps.ServiceNow.Models.Content;

/// <summary>The structured result of reading a translated roundtrip file back into objects.</summary>
public class ParsedArticleFile
{
    /// <summary>The main article id, read from the head meta (blackbird-entry-id / blackbird-ucid).</summary>
    public string? MainEntryId { get; set; }

    /// <summary>The file's locale, read from blackbird-locale or the html lang attribute.</summary>
    public string? MainLocale { get; set; }

    /// <summary>One entry per object container found in the body.</summary>
    public List<ParsedEntry> Entries { get; set; } = new();
}

public class ParsedEntry
{
    public string EntryId { get; set; } = string.Empty;
    public List<ParsedField> Fields { get; set; } = new();
}

public class ParsedField
{
    public string FieldId { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
    public string Value { get; set; } = string.Empty;
}
