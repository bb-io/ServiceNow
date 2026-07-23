namespace Apps.ServiceNow.Constants;

public static class TableFields
{
    public const string Article =
        "sys_id,number,short_description,workflow_state,kb_knowledge_base,kb_category,language,author,article_type,sys_created_on,sys_updated_on,text";

    public const string Incident =
        "sys_id,number,short_description,description,state,priority,urgency,impact,caller_id,assigned_to,sys_created_on,sys_updated_on";
}
