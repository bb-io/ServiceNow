using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Models.Polling;
using Apps.ServiceNow.Models.Requests;
using Apps.ServiceNow.Models.Responses;
using Apps.ServiceNow.Utils;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;

namespace Apps.ServiceNow.Events;

[PollingEventList]
public class ArticlePollingList(InvocationContext invocationContext) : Invocable(invocationContext)
{
    private const string ArticleFields = TableFields.Article;

    private const string StatusFields = "sys_id,number,short_description,workflow_state,language,kb_knowledge_base,sys_updated_on";

    [PollingEvent("On articles created or updated",
        Description = "Triggered on an interval and outputs the knowledge articles created or updated since the previous poll.")]
    public async Task<PollingEventResponse<PollingMemory, ArticlesEventResponse>> OnArticlesCreatedOrUpdated(
        PollingEventRequest<PollingMemory> request,
        [PollingEventParameter] ArticleCreatedOrUpdatedFilter filter)
    {
        if (request.Memory?.LastPollingTime is null)
            return Baseline<ArticlesEventResponse>();

        var since = request.Memory.LastPollingTime.Value;

        var clauses = BuildArticleScopeClauses(filter.ArticleId, filter.Language, filter.KnowledgeBaseIds);
        clauses.Add($"sys_updated_on>{ServiceNowDate.Format(since)}");
        clauses.Add("ORDERBYsys_updated_on");

        var query = new Dictionary<string, string>
        {
            ["sysparm_query"] = string.Join("^", clauses),
            ["sysparm_fields"] = ArticleFields
        };

        var dtos = await Client.SearchTableAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, query);

        var items = dtos
            .Select(dto => new ArticleEventItem(dto, ClassifyEventType(dto, since)))
            .ToList();

        return new PollingEventResponse<PollingMemory, ArticlesEventResponse>
        {
            FlyBird = items.Count > 0,
            Memory = new PollingMemory { LastPollingTime = DateTime.UtcNow },
            Result = items.Count > 0
                ? new ArticlesEventResponse { Articles = items, TotalCount = items.Count }
                : null
        };
    }

    [PollingEvent("On articles status changed",
        Description = "Triggered on an interval and outputs knowledge articles whose workflow state (draft, review, published, retired) changed since the previous poll.")]
    public async Task<PollingEventResponse<ArticleStatePollingMemory, ArticleStatusChangedEventResponse>> OnArticleStatusChanged(
        PollingEventRequest<ArticleStatePollingMemory> request,
        [PollingEventParameter] ArticleStatusChangedFilter filter)
    {
        var clauses = BuildArticleScopeClauses(filter.ArticleId, filter.Language, filter.KnowledgeBaseIds);
        var query = new Dictionary<string, string> { ["sysparm_fields"] = StatusFields };
        if (clauses.Count > 0)
            query["sysparm_query"] = string.Join("^", clauses);

        var dtos = await Client.SearchTableAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, query);
        var snapshot = dtos
            .GroupBy(d => d.SysId)
            .ToDictionary(g => g.Key, g => g.Last());
        var currentStates = snapshot.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.WorkflowState ?? string.Empty);

        if (request.Memory?.LastPollingTime is null)
            return new PollingEventResponse<ArticleStatePollingMemory, ArticleStatusChangedEventResponse>
            {
                FlyBird = false,
                Memory = new ArticleStatePollingMemory { LastPollingTime = DateTime.UtcNow, ArticleStates = currentStates },
                Result = null
            };

        var wantedStatuses = filter.Statuses?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        var changes = new List<ArticleStatusChangeItem>();

        foreach (var (id, dto) in snapshot)
        {
            if (!request.Memory.ArticleStates.TryGetValue(id, out var previous))
                continue;

            var current = dto.WorkflowState ?? string.Empty;
            if (string.Equals(previous, current, StringComparison.OrdinalIgnoreCase))
                continue;

            if (wantedStatuses is { Count: > 0 } &&
                !wantedStatuses.Contains(current, StringComparer.OrdinalIgnoreCase))
                continue;

            changes.Add(new ArticleStatusChangeItem(dto, previous));
        }

        return new PollingEventResponse<ArticleStatePollingMemory, ArticleStatusChangedEventResponse>
        {
            FlyBird = changes.Count > 0,
            Memory = new ArticleStatePollingMemory { LastPollingTime = DateTime.UtcNow, ArticleStates = currentStates },
            Result = changes.Count > 0
                ? new ArticleStatusChangedEventResponse { Articles = changes, TotalCount = changes.Count }
                : null
        };
    }

    private static List<string> BuildArticleScopeClauses(
        string? articleId, string? language, IEnumerable<string>? knowledgeBaseIds)
    {
        var clauses = new List<string>();

        if (!string.IsNullOrWhiteSpace(articleId))
            clauses.Add($"sys_id={articleId}");
        if (!string.IsNullOrWhiteSpace(language))
            clauses.Add($"language={language}");

        var kbIds = knowledgeBaseIds?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (kbIds is { Count: > 0 })
            clauses.Add($"kb_knowledge_baseIN{string.Join(",", kbIds)}");

        return clauses;
    }

    private static string ClassifyEventType(ArticleDto dto, DateTime since)
    {
        var created = ServiceNowDate.Parse(dto.CreatedOn);
        return created.HasValue && created.Value > since ? "created" : "updated";
    }
}
