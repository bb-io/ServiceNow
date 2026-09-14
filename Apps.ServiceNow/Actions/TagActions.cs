using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Extensions;
using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Models.Identifiers;
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
    [Action("Search tags", Description = "Search all available tags.")]
    public async Task<SearchTagsResponse> SearchTags([ActionParameter] SearchTagsRequest input)
    {
        var queryDict = new Dictionary<string, string>
        {
            { "viewable_by", "everyone" },
        };
        
        if (!string.IsNullOrEmpty(input.NameContains))
            queryDict.Add("sysparm_query", $"nameLIKE{input.NameContains}");

        var response = await Client.SearchTableAsync<TagDto>(ApiEndpoints.LabelTable, queryDict);
        return new(response.Select(x => new TagResponse(x)).ToArray());
    }
    
    [Action("Get tags", Description = "Get details for a specific tag.")]
    public async Task<TagResponse> GetTag([ActionParameter] TagIdentifier tagInput)
    {
        var request = new RestRequest($"/api/now/table/label/{tagInput.TagId}");
        var response = await Client.ExecuteWithErrorHandling<ResultWrapper<TagDto>>(request);

        return new(response.Result);
    }

    [Action("Create tag", Description = "Create a new tag.")]
    public async Task<TagResponse> CreateTag([ActionParameter] CreateTagRequest createInput)
    {
        var body = new Dictionary<string, object>
            {
                ["name"] = createInput.TagName,
                ["type"] = "standard",
            }
            .AddIfNotEmpty("short_description", createInput.ShortDescription)
            .AddIfNotEmpty("color", createInput.Color);

        var dto = await Client.CreateRecordAsync<TagDto>(ApiEndpoints.LabelTable, body);
        return new(dto);
    }
    
    [Action("Delete tag", Description = "Delete an existing tag.")]
    public async Task DeleteTag([ActionParameter] TagIdentifier tagInput)
    {
        await Client.DeleteRecordAsync(ApiEndpoints.LabelTable, tagInput.TagId);
    }
}