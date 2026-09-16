using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Requests.Tag;

public class CreateTagRequest
{
    [Display("Tag name")] 
    public string TagName { get; set; } = string.Empty;

    [Display("Short description")]
    public string? ShortDescription { get; set; }

    [Display("Color")]
    public string? Color { get; set; }
}