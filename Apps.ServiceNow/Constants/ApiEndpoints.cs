namespace Apps.ServiceNow.Constants;

public static class ApiEndpoints
{
    // Table API
    public const string Table = "/api/now/table";
    public const string KnowledgeTable = $"{Table}/kb_knowledge";
    public const string KnowledgeBaseTable = $"{Table}/kb_knowledge_base";
    public const string LanguageTable = $"{Table}/sys_language";
    public const string UserTable = $"{Table}/sys_user";
    public const string IncidentTable = $"{Table}/incident";
    public const string JournalTable = $"{Table}/sys_journal_field";

    // Knowledge Management API
    public const string KnowledgeArticles = "/api/sn_km_api/knowledge/articles";

    // Attachment API
    public const string Attachment = "/api/now/attachment";
}
