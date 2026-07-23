namespace Apps.ServiceNow.Constants;

/// <summary>
/// The vocabulary of the self-describing translatable HTML file — the contract shared by the
/// "Download article" (writer) and "Upload article" (reader) roundtrip actions. Every attribute
/// the download side emits exists so the upload side (or Blacklake) can read it back.
/// </summary>
public static class RoundtripHtml
{
    /// <summary>System name used in metadata, ITS provenance and the target-system reference.</summary>
    public const string SystemName = "ServiceNow";

    // --- container + field attributes (system-prefixed per the roundtrip contract) ---
    public const string EntryIdAttr = "data-servicenow-entry-id";
    public const string FieldIdAttr = "data-servicenow-field-id";
    public const string FieldTypeAttr = "data-servicenow-field-type";
    public const string HtmlAttr = "data-servicenow-html";

    // --- cross-app / Blacklake attributes ---
    public const string BlackbirdKeyAttr = "data-blackbird-key";
    public const string BlackbirdSizeAttr = "data-blackbird-size";

    // --- head meta names (without the "blackbird-" prefix; the writer prepends it) ---
    public const string MetaEntryId = "entry-id";
    public const string MetaLocale = "locale";
    public const string MetaUcid = "ucid";
    public const string MetaContentName = "content-name";
    public const string MetaAdminUrl = "admin-url";
    public const string MetaPublicUrl = "public-url";
    public const string MetaSystemName = "system-name";
    public const string MetaSystemRef = "system-ref";

    // --- ServiceNow localizable field ids on the kb_knowledge table ---
    public const string TitleFieldId = "short_description";
    public const string BodyFieldId = "text";

    // --- field-type tokens written to data-servicenow-field-type ---
    public const string StringType = "string";
    public const string HtmlType = "html";

    /// <summary>
    /// ServiceNow's default maximum length for the short_description column. Emitted as a soft
    /// size hint for downstream translation apps; not enforced by this app.
    /// </summary>
    public const int TitleMaxLength = 160;

    /// <summary>Media type used for XLIFF 2.x output when the uploaded file was bilingual.</summary>
    public const string Xliff2MediaType = "application/xliff+xml";
}
