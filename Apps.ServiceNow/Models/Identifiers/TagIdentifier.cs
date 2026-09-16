using Apps.ServiceNow.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.ServiceNow.Models.Identifiers;

public class TagIdentifier
{
    [Display("Tag ID"), DataSource(typeof(TagDataHandler))]
    public string TagId { get; set; } = string.Empty;
}