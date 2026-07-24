using System.Text;
using System.Text.RegularExpressions;

namespace Apps.ServiceNow.Utils;

public static class FileNameSanitizer
{
    private const int MaxLength = 120;

    public static string Sanitize(string? name, string? fallback)
    {
        var basis = string.IsNullOrWhiteSpace(name) ? fallback : name;
        if (string.IsNullOrWhiteSpace(basis))
            basis = "article";

        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(basis!.Length);
        foreach (var ch in basis.Trim())
            sb.Append(invalid.Contains(ch) ? '_' : ch);

        var cleaned = Regex.Replace(sb.ToString(), @"\s+", "_").Trim('_', '.');
        if (cleaned.Length == 0)
            cleaned = string.IsNullOrWhiteSpace(fallback) ? "article" : fallback!;

        return cleaned.Length > MaxLength ? cleaned[..MaxLength] : cleaned;
    }
}
