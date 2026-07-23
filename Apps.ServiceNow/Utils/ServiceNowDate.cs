using System.Globalization;

namespace Apps.ServiceNow.Utils;

public static class ServiceNowDate
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

    /// <summary>Formats a date as ServiceNow's UTC datetime string used in encoded queries and fields.</summary>
    public static string Format(DateTime value) => value.ToString(DateFormat, CultureInfo.InvariantCulture);

    /// <summary>Parses a ServiceNow "yyyy-MM-dd HH:mm:ss" datetime string (UTC). Returns null when unparseable.</summary>
    public static DateTime? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            return parsed;
        return DateTime.TryParse(value, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var fallback)
            ? fallback
            : null;
    }
}
