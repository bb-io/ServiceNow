using Apps.ServiceNow.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Responses;

public class ArticleSearchItem
{
    public ArticleSearchItem() { }

    public ArticleSearchItem(KmSearchArticleDto dto)
    {
        var id = dto.Id ?? string.Empty;
        var colon = id.IndexOf(':');
        ArticleId = colon >= 0 ? id[(colon + 1)..] : id;
        Number = dto.Number ?? string.Empty;
        Title = dto.Title ?? string.Empty;
        Snippet = dto.Snippet ?? string.Empty;
        Score = dto.Score;
    }

    [Display("Article ID")] public string ArticleId { get; set; } = string.Empty;
    [Display("Number")] public string Number { get; set; } = string.Empty;
    [Display("Title")] public string Title { get; set; } = string.Empty;
    [Display("Snippet", Description = "A short preview of the matching text.")] public string Snippet { get; set; } = string.Empty;
    [Display("Score", Description = "Relevance score. -1 when no search text was supplied.")] public double Score { get; set; }
}

public class SearchArticlesResponse
{
    [Display("Articles")] public List<ArticleSearchItem> Articles { get; set; } = new();
    [Display("Total count", Description = "The number of articles returned.")] public int TotalCount { get; set; }
}
