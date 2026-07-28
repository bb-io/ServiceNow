using System.Globalization;

namespace Apps.ServiceNow.Utils;

public static class ServiceNowDate
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

    public static string Format(DateTime value) => value.ToString(DateFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// Normalises a value to UTC. An unspecified kind is taken as UTC, which is what the date inputs document and
    /// how polling memory is written. Needed because a value that round-trips through serialised memory can come
    /// back as a local time, and ServiceNow stores and compares these fields in UTC.
    /// </summary>
    public static DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value;

    /// <summary>
    /// Formats a value for an encoded-query date comparison. ServiceNow compares such a plain date string against
    /// the field's stored UTC value, so a local time has to be converted first.
    /// </summary>
    public static string FormatUtc(DateTime value) => Format(ToUtc(value));

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
