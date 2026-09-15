using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Models.Dtos.Label;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.ServiceNow.Api;

public partial class Client
{
    private const int TablePageSize = 100;

    public async Task<T> GetRecordAsync<T>(string table, string sysId, IReadOnlyDictionary<string, string>? query = null)
    {
        var request = new RestRequest($"{table}/{sysId}", Method.Get);
        AddQuery(request, query);
        var wrapper = await ExecuteWithErrorHandling<ResultWrapper<T>>(request);
        return wrapper.Result;
    }

    public async Task<T> CreateRecordAsync<T>(string table, object body, IReadOnlyDictionary<string, string>? query = null)
    {
        var request = new RestRequest(table, Method.Post).AddJsonBody(body);
        AddQuery(request, query);
        var wrapper = await ExecuteWithErrorHandling<ResultWrapper<T>>(request);
        return wrapper.Result;
    }

    public async Task<T> UpdateRecordAsync<T>(string table, string sysId, object body, IReadOnlyDictionary<string, string>? query = null)
    {
        var request = new RestRequest($"{table}/{sysId}", Method.Patch).AddJsonBody(body);
        AddQuery(request, query);
        var wrapper = await ExecuteWithErrorHandling<ResultWrapper<T>>(request);
        return wrapper.Result;
    }

    public async Task DeleteRecordAsync(string table, string sysId)
    {
        var response = await ExecuteAsync(new RestRequest($"{table}/{sysId}", Method.Delete));
        if (!response.IsSuccessStatusCode)
            throw ConfigureErrorException(response);
    }

    public async Task<List<T>> SearchTableAsync<T>(string table, IReadOnlyDictionary<string, string> query, int? max = null)
    {
        var results = new List<T>();
        var offset = 0;

        while (true)
        {
            var pageSize = max.HasValue ? Math.Min(TablePageSize, max.Value - results.Count) : TablePageSize;
            if (pageSize <= 0) break;

            var request = new RestRequest(table, Method.Get);
            AddQuery(request, query);
            request.AddQueryParameter("sysparm_limit", pageSize.ToString());
            request.AddQueryParameter("sysparm_offset", offset.ToString());

            var response = await ExecuteAsync(request);
            if (!response.IsSuccessStatusCode)
                throw ConfigureErrorException(response);

            var page = JsonConvert.DeserializeObject<ResultListWrapper<T>>(response.Content!)?.Result ?? new List<T>();
            results.AddRange(page);

            if (page.Count < pageSize) break;

            var total = ReadTotalCount(response);
            offset += pageSize;

            if (total.HasValue && results.Count >= total.Value) break;
            if (max.HasValue && results.Count >= max.Value) break;
        }

        return max.HasValue ? results.Take(max.Value).ToList() : results;
    }

    public async Task<Dictionary<string, HashSet<string>>> GetArticleTagsAsync(
        IReadOnlyCollection<string> articleIds, 
        IReadOnlyCollection<string>? onlyTags = null)
    {
        var map = new Dictionary<string, HashSet<string>>();

        foreach (var chunk in articleIds.Chunk(100))
        {
            var clauses = new List<string>
            {
                $"table={TableNames.Knowledge}",
                $"table_keyIN{string.Join(",", chunk)}",
            };

            if (onlyTags is { Count: > 0 })
                clauses.Add($"labelIN{string.Join(",", onlyTags)}");

            var searchBody = new Dictionary<string, string>
            {
                ["sysparm_query"] = string.Join("^", clauses),
                ["sysparm_fields"] = "table_key,label",
                ["sysparm_exclude_reference_link"] = "true",
            };
            var entries = await SearchTableAsync<LabelEntryDto>(ApiEndpoints.LabelEntryTable, searchBody);

            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.TableKey) || string.IsNullOrWhiteSpace(entry.Label?.Value))
                    continue;

                if (!map.TryGetValue(entry.TableKey, out var tags))
                    map[entry.TableKey] = tags = new HashSet<string>();

                tags.Add(entry.Label.Value);
            }
        }

        return map;
    }
    
    public async Task<List<AttachmentDto>> ListAttachmentsAsync(string table, string recordId)
    {
        var request = new RestRequest(ApiEndpoints.Attachment, Method.Get)
            .AddQueryParameter("sysparm_query", $"table_name={table}^table_sys_id={recordId}")
            .AddQueryParameter("sysparm_fields", "sys_id,file_name,content_type,size_bytes,table_name,table_sys_id,download_link");

        var wrapper = await ExecuteWithErrorHandling<ResultListWrapper<AttachmentDto>>(request);
        return wrapper.Result;
    }

    public async Task<AttachmentDto> GetAttachmentMetadataAsync(string attachmentId)
    {
        var wrapper = await ExecuteWithErrorHandling<ResultWrapper<AttachmentDto>>(
            new RestRequest($"{ApiEndpoints.Attachment}/{attachmentId}", Method.Get));
        return wrapper.Result;
    }

    public async Task<(byte[] Bytes, string ContentType)> DownloadAttachmentAsync(string attachmentId)
    {
        var response = await ExecuteAsync(new RestRequest($"{ApiEndpoints.Attachment}/{attachmentId}/file", Method.Get));
        if (!response.IsSuccessStatusCode)
            throw ConfigureErrorException(response);

        var contentType = response.ContentType ?? "application/octet-stream";
        return (response.RawBytes ?? Array.Empty<byte>(), contentType);
    }

    private static void AddQuery(RestRequest request, IReadOnlyDictionary<string, string>? query)
    {
        if (query == null) return;
        foreach (var kvp in query)
            request.AddQueryParameter(kvp.Key, kvp.Value);
    }

    private static int? ReadTotalCount(RestResponse response)
    {
        var header = response.Headers?.FirstOrDefault(h =>
            string.Equals(h.Name, "X-Total-Count", StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
        return int.TryParse(header, out var total) ? total : null;
    }
}
