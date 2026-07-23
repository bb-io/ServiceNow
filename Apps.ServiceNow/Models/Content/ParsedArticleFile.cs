namespace Apps.ServiceNow.Models.Content;

public class ParsedArticleFile
{
    public string? MainEntryId { get; set; }

    public string? MainLocale { get; set; }

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
