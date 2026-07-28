using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.ServiceNow.Handlers;

public class AttachmentDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    private const int MaxItems = 30;

    /// <summary>
    /// sys_attachment is shared by the whole platform: on a stock instance the overwhelming majority of its rows are
    /// internal files such as certificate revocation lists and UI thumbnails. Only the records this app works with
    /// are worth offering, otherwise the dropdown is unusable.
    /// </summary>
    private static readonly string[] ParentTables = [TableNames.Incident, TableNames.Knowledge];

    private static readonly Dictionary<string, string> ParentLabels = new()
    {
        [TableNames.Incident] = "incident",
        [TableNames.Knowledge] = "article"
    };

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context, CancellationToken cancellationToken)
    {
        var clauses = new List<string> { $"table_nameIN{string.Join(",", ParentTables)}" };

        if (!string.IsNullOrWhiteSpace(context.SearchString))
            clauses.Add($"file_nameLIKE{context.SearchString}");

        clauses.Add("ORDERBYDESCsys_created_on");

        var query = new Dictionary<string, string>
        {
            ["sysparm_fields"] = "sys_id,file_name,table_name,table_sys_id",
            ["sysparm_query"] = string.Join("^", clauses)
        };

        var attachments = await Client.SearchTableAsync<AttachmentDto>(ApiEndpoints.AttachmentTable, query, MaxItems);
        var attached = attachments.Where(x => !string.IsNullOrWhiteSpace(x.SysId)).ToList();

        var parentNumbers = await ResolveParentNumbersAsync(attached);

        return attached.Select(x => new DataSourceItem(x.SysId, BuildDisplayName(x, parentNumbers)));
    }

    /// <summary>
    /// Maps each parent record's sys_id to its number, so an attachment named "Pasted Image" can still be told apart
    /// from the next one. One extra request per parent table, not per attachment.
    /// </summary>
    private async Task<Dictionary<string, string>> ResolveParentNumbersAsync(IEnumerable<AttachmentDto> attachments)
    {
        var numbers = new Dictionary<string, string>();

        var groups = attachments
            .Where(x => !string.IsNullOrWhiteSpace(x.TableName) && !string.IsNullOrWhiteSpace(x.TableSysId))
            .GroupBy(x => x.TableName!);

        foreach (var group in groups)
        {
            var ids = group.Select(x => x.TableSysId!).Distinct().ToList();

            var records = await Client.SearchTableAsync<NumberedRecordDto>(ApiEndpoints.TableFor(group.Key),
                new Dictionary<string, string>
                {
                    ["sysparm_fields"] = "sys_id,number",
                    ["sysparm_query"] = $"sys_idIN{string.Join(",", ids)}"
                });

            foreach (var record in records.Where(r => !string.IsNullOrWhiteSpace(r.SysId)))
                numbers[record.SysId!] = record.Number ?? string.Empty;
        }

        return numbers;
    }

    private static string BuildDisplayName(AttachmentDto attachment, IReadOnlyDictionary<string, string> parentNumbers)
    {
        var fileName = string.IsNullOrWhiteSpace(attachment.FileName) ? attachment.SysId : attachment.FileName!;

        var parent = attachment.TableSysId is { } parentId
                     && parentNumbers.TryGetValue(parentId, out var number)
                     && !string.IsNullOrWhiteSpace(number)
            ? number
            : attachment.TableName is { } table && ParentLabels.TryGetValue(table, out var label)
                ? label
                : null;

        return parent is null ? fileName : $"{fileName} ({parent})";
    }
}
