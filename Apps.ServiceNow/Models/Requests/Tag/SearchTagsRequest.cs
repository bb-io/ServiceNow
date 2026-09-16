using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Requests.Tag;

public class SearchTagsRequest
{
    [Display("Tag name contains")]
    public string? NameContains { get; set; }
}