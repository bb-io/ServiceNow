namespace Apps.ServiceNow.Models.Content;

public class ArticleHtmlModel
{
    public string EntryId { get; set; } = string.Empty;

    public string Locale { get; set; } = "en";

    public string? Title { get; set; }

    public string? Body { get; set; }

    public string? AdminUrl { get; set; }
    public string? PublicUrl { get; set; }
    public string? SystemRef { get; set; }

    public string? UpdatedByName { get; set; }
}
