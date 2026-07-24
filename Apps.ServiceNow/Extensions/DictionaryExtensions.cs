namespace Apps.ServiceNow.Extensions;

public static class DictionaryExtensions
{
    /// <summary>Adds the value under <paramref name="key"/> when it is not null. Empty strings are kept.</summary>
    public static Dictionary<string, object> AddIfNotNull(
        this Dictionary<string, object> dict, string key, string? value)
    {
        if (value is not null)
            dict[key] = value;
        return dict;
    }
    
    /// <summary>Adds the value under <paramref name="key"/> when it is not null. Empty strings are kept.</summary>
    public static Dictionary<string, string> AddIfNotNull(
        this Dictionary<string, string> dict, string key, string? value)
    {
        if (value is not null)
            dict[key] = value;
        return dict;
    }

    /// <summary>Adds the value under <paramref name="key"/> when it is not null, empty or whitespace.</summary>
    public static Dictionary<string, object> AddIfNotEmpty(
        this Dictionary<string, object> dict, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            dict[key] = value;
        return dict;
    }
}
