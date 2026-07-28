namespace Apps.ServiceNow.Constants;

public static class ApiEndpoints
{
    public const string Table = "/api/now/table";
    public const string KnowledgeTable = $"{Table}/{TableNames.Knowledge}";
    public const string KnowledgeBaseTable = $"{Table}/{TableNames.KnowledgeBase}";
    public const string LanguageTable = $"{Table}/{TableNames.Language}";
    public const string UserTable = $"{Table}/{TableNames.User}";
    public const string IncidentTable = $"{Table}/{TableNames.Incident}";
    public const string JournalTable = $"{Table}/{TableNames.Journal}";
    public const string AttachmentTable = $"{Table}/{TableNames.Attachment}";

    public const string KnowledgeArticles = "/api/sn_km_api/knowledge/articles";

    public const string Attachment = "/api/now/attachment";

    public static string TableFor(string tableName) => $"{Table}/{tableName}";
}
