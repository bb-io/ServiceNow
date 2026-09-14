using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Models.Requests.Tag;
using Apps.ServiceNow.Models.Responses.Tag;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.ServiceNow.Actions;

[ActionList("Tags")]
public class TagActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Search tags", Description = "Search all available tags")]
    public async Task<SearchTagsResponse> SearchTags([ActionParameter] SearchTagsRequest input)
    {
        var query = new List<string> { "viewable_by=everyone" };
        
        if (!string.IsNullOrEmpty(input.NameContains))
            query.Add($"nameLIKE{input.NameContains}");
        
        var request = new RestRequest("/api/now/table/label").AddQueryParameter("sysparm_query", string.Join('^', query));
        var response = await Client.ExecuteWithErrorHandling<ResultListWrapper<TagDto>>(request);

        var tags = response.Result.Select(x => new TagResponse(x)).ToArray();
        return new(tags);
    }
}