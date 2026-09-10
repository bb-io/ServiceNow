namespace Apps.ServiceNow.Extensions;

public static class StringExtensions
{
    public static string? ToAbsoluteUrlString(this string value, Uri instanceBaseUrl) 
    {
        return value.IsInstanceRelativeUrl() && Uri.TryCreate(instanceBaseUrl, value, out var absolute)
            ? absolute.ToString()
            : null;
    }
    
    public static string? ToRelativeUrlString(this string value, Uri instanceBaseUrl)
    {
        string origin = instanceBaseUrl.GetLeftPart(UriPartial.Authority);

        return value.StartsWith(origin, StringComparison.OrdinalIgnoreCase)
            ? value[origin.Length..]
            : null;
    }
    
    private static bool IsInstanceRelativeUrl(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) 
            return false;
        
        if (value.StartsWith('#'))
            return false;
        
        if (value.StartsWith('/'))
            return true;
        
        return !Uri.TryCreate(value, UriKind.Absolute, out _);
    }
}