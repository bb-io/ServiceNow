using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Extensions;
using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Models.Dtos.Label;
using Apps.ServiceNow.Models.Identifiers;
using Apps.ServiceNow.Models.Requests.Tag;
using Apps.ServiceNow.Models.Responses.Tag;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
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

    [Action("Add tag to article", Description = "Add a tag to an existing article.")]
    public async Task AddTagToArticle(
        [ActionParameter] TagIdentifier tagInput,
        [ActionParameter] ArticleIdentifier articleInput)
    {
        var body = new
        {
            label = tagInput.TagId,
            table = "kb_knowledge",
            table_key = articleInput.ArticleId
        };

        var dto = await Client.CreateRecordAsync<LabelEntryDto>(ApiEndpoints.LabelEntryTable, body);
        if (dto.Label is null && string.IsNullOrWhiteSpace(dto.TableKey))
        {
            throw new PluginMisconfigurationException(
                "ServiceNow discarded the tag assignment. " +
                "The instance is missing the write permissions for the 'label_entry' table. " +
                "Please add the necessary 'create' ACL permissions for the user to perform this action: " +
                "'label_entry.table', 'label_entry.table_key' and 'label_entry.label'.");
        }
    }
    
    [Action("Remove tag from article", Description = "Remove a tag from an article.")]
    public async Task RemoveTagFromArticle(
        [ActionParameter] TagIdentifier tagInput,
        [ActionParameter] ArticleIdentifier articleInput)
    {
        var body = new Dictionary<string, string>
        {
            ["sysparm_query"] = $"table={TableNames.Knowledge}^table_key={articleInput.ArticleId}^label={tagInput.TagId}",
            ["sysparm_fields"] = "sys_id",
        };
        var entries = await Client.SearchTableAsync<LabelEntryDto>(ApiEndpoints.LabelEntryTable, body);
        
        foreach (var entry in entries)
            await Client.DeleteRecordAsync(ApiEndpoints.LabelEntryTable, entry.SysId);
    }
}