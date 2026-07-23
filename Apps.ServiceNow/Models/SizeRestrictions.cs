using Newtonsoft.Json;

namespace Apps.ServiceNow.Models;

public class SizeRestrictions
{
    [JsonProperty("MinimumSize", NullValueHandling = NullValueHandling.Ignore)]
    public int? MinimumSize { get; set; }

    [JsonProperty("MaximumSize", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaximumSize { get; set; }
}

public static class SizeRestrictionHelper
{
    public static string? Serialize(SizeRestrictions? restrictions)
    {
        if (restrictions is null || (restrictions.MinimumSize is null && restrictions.MaximumSize is null))
            return null;
        return JsonConvert.SerializeObject(restrictions);
    }
}
