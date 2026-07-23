using Newtonsoft.Json;

namespace Apps.ServiceNow.Models;

/// <summary>
/// CMS-enforced length limits for a field, serialized into <c>data-blackbird-size</c> so downstream
/// translation apps don't overflow a short-text field. Only the non-null bounds are serialized.
/// </summary>
public class SizeRestrictions
{
    [JsonProperty("MinimumSize", NullValueHandling = NullValueHandling.Ignore)]
    public int? MinimumSize { get; set; }

    [JsonProperty("MaximumSize", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaximumSize { get; set; }
}

public static class SizeRestrictionHelper
{
    /// <summary>Serializes the restrictions, or returns null when there is nothing to serialize.</summary>
    public static string? Serialize(SizeRestrictions? restrictions)
    {
        if (restrictions is null || (restrictions.MinimumSize is null && restrictions.MaximumSize is null))
            return null;
        return JsonConvert.SerializeObject(restrictions);
    }
}
