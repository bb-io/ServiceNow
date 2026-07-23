using System.Net.Mime;
using System.Text;
using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Content;
using Apps.ServiceNow.Models.Dtos;
using Apps.ServiceNow.Models.Identifiers;
using Apps.ServiceNow.Models.Requests;
using Apps.ServiceNow.Models.Responses;
using Apps.ServiceNow.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;

namespace Apps.ServiceNow.Actions;

[ActionList("Articles")]
public class ArticleActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : Invocable(invocationContext)
{
    private const string MetadataFields =
        "sys_id,number,short_description,workflow_state,kb_knowledge_base,kb_category,language,author,article_type,sys_created_on,sys_updated_on,text";

    [Action("Search articles", Description = "Find knowledge articles matching a search text and optional filters.")]
    public async Task<SearchArticlesResponse> SearchArticles([ActionParameter] SearchArticlesRequest request)
    {
        var query = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.Query))
            query["query"] = request.Query;
        if (!string.IsNullOrWhiteSpace(request.Language))
            query["language"] = request.Language;

        var kbIds = request.KnowledgeBaseIds?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (kbIds is { Count: > 0 })
            query["kb"] = string.Join(",", kbIds);

        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.State))
            filters.Add($"workflow_state={request.State}");
        if (request.CreatedAfter.HasValue)
            filters.Add($"sys_created_on>{ServiceNowDate.Format(request.CreatedAfter.Value)}");
        if (request.CreatedBefore.HasValue)
            filters.Add($"sys_created_on<{ServiceNowDate.Format(request.CreatedBefore.Value)}");
        if (request.UpdatedAfter.HasValue)
            filters.Add($"sys_updated_on>{ServiceNowDate.Format(request.UpdatedAfter.Value)}");
        if (request.UpdatedBefore.HasValue)
            filters.Add($"sys_updated_on<{ServiceNowDate.Format(request.UpdatedBefore.Value)}");
        if (filters.Count > 0)
            query["filter"] = string.Join("^", filters);

        if (request.Limit is <= 0)
            throw new PluginMisconfigurationException("The 'Maximum results' value must be greater than zero.");

        var articles = await Client.SearchArticlesAsync(query, request.Limit);
        var items = articles.Select(x => new ArticleSearchItem(x)).ToList();

        return new SearchArticlesResponse { Articles = items, TotalCount = items.Count };
    }

    [Action("Get article metadata", Description = "Read all metadata fields of a single knowledge article.")]
    public async Task<ArticleMetadataResponse> GetArticleMetadata([ActionParameter] ArticleIdentifier identifier)
    {
        ValidateArticleId(identifier.ArticleId);
        var dto = await Client.GetRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, identifier.ArticleId,
            new Dictionary<string, string> { ["sysparm_fields"] = MetadataFields });
        return new ArticleMetadataResponse(dto);
    }

    [Action("Update article metadata", Description = "Change one or more metadata fields of an existing article.")]
    public async Task<ArticleMetadataResponse> UpdateArticleMetadata([ActionParameter] UpdateArticleRequest request)
    {
        ValidateArticleId(request.ArticleId);

        var body = new Dictionary<string, object>();
        if (request.Title is not null) body["short_description"] = request.Title;
        if (request.Body is not null) body["text"] = request.Body;
        if (!string.IsNullOrWhiteSpace(request.KnowledgeBaseId)) body["kb_knowledge_base"] = request.KnowledgeBaseId;
        if (!string.IsNullOrWhiteSpace(request.CategoryId)) body["kb_category"] = request.CategoryId;
        if (!string.IsNullOrWhiteSpace(request.Language)) body["language"] = request.Language;

        if (body.Count == 0)
            throw new PluginMisconfigurationException(
                "No changes were provided. Fill in at least one field to update on the article.");

        var dto = await Client.UpdateRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, request.ArticleId, body,
            new Dictionary<string, string> { ["sysparm_fields"] = MetadataFields });
        return new ArticleMetadataResponse(dto);
    }

    // Raw (writable) kb_knowledge fields used by the translation roundtrip. The roundtrip reads the
    // Table API `text` column (not the KM rendered `content`) so the same value can be written back.
    private const string RoundtripFields = "sys_id,number,short_description,text,language,workflow_state";

    [BlueprintActionDefinition(BlueprintAction.DownloadContent)]
    [Action("Download article", Description = "Export an article's translatable fields (title and body) as a self-describing HTML file ready for translation.")]
    public async Task<DownloadContentOutput> DownloadArticle([ActionParameter] DownloadArticleRequest request)
    {
        ValidateArticleId(request.ContentId);

        var dto = await Client.GetRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, request.ContentId,
            new Dictionary<string, string> { ["sysparm_fields"] = RoundtripFields });

        var locale = !string.IsNullOrWhiteSpace(request.Locale) ? request.Locale!
            : !string.IsNullOrWhiteSpace(dto.Language) ? dto.Language!
            : "en";

        var model = new ArticleHtmlModel
        {
            EntryId = dto.SysId,
            Locale = locale,
            Title = dto.ShortDescription,
            Body = dto.Text,
            AdminUrl = Client.GetArticleAdminUrl(dto.SysId),
            PublicUrl = Client.GetArticlePublicUrl(dto.Number),
            SystemRef = Client.InstanceBaseUrl.ToString().TrimEnd('/')
        };

        var html = ArticleHtmlConverter.ToHtml(model);
        var safeName = FileNameSanitizer.Sanitize(dto.ShortDescription ?? dto.Number, dto.SysId);
        var fileName = $"{safeName}_{locale}.html";

        var file = await fileManagementClient.UploadAsync(
            new MemoryStream(Encoding.UTF8.GetBytes(html)), MediaTypeNames.Text.Html, fileName);

        return new DownloadContentOutput { Content = file, RootEntryId = dto.SysId };
    }

    [BlueprintActionDefinition(BlueprintAction.UploadContent)]
    [Action("Upload article", Description = "Import a translated file (.html/.xliff/.xlf) and write its title and body back onto the matching knowledge article.")]
    public async Task<DownloadContentOutput> UploadArticle([ActionParameter] UploadArticleRequest input)
    {
        if (input.Content is null)
            throw new PluginMisconfigurationException("Please provide a file for 'File'.");

        var name = input.Content.Name ?? string.Empty;
        if (!name.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
            && !name.EndsWith(".xliff", StringComparison.OrdinalIgnoreCase)
            && !name.EndsWith(".xlf", StringComparison.OrdinalIgnoreCase))
            throw new PluginMisconfigurationException("Only .html, .xliff and .xlf files are supported.");

        var errors = new List<ContentProcessingError>();

        // 1. Load the file through the universal Transformation wrapper (handles bilingual + monolingual).
        var download = await fileManagementClient.DownloadAsync(input.Content);
        var bytes = await download.GetByteData();
        var loadResult = Transformation.Load(new MemoryStream(bytes), name, input.Content.ContentType);

        var rawHtml = Encoding.UTF8.GetString(bytes);
        string content;
        if (loadResult.Success)
        {
            var target = loadResult.Target();
            content = target.Success
                ? target.Value.ToStream(MetadataHandling.Include).ReadString()
                : rawHtml;
        }
        else
        {
            content = rawHtml;
        }

        // 2. Parse back into objects; fall back to the raw HTML if the wrapper stripped our structure.
        var parsed = ArticleHtmlConverter.ParseHtml(content);
        if (parsed.Entries.Count == 0 && !ReferenceEquals(content, rawHtml))
            parsed = ArticleHtmlConverter.ParseHtml(rawHtml);

        // 3. Reconstruct each object and write only the changed fields back.
        foreach (var entry in parsed.Entries)
        {
            var targetId = !string.IsNullOrWhiteSpace(input.ContentId) ? input.ContentId : entry.EntryId;
            if (string.IsNullOrWhiteSpace(targetId))
            {
                errors.Add(new ContentProcessingError
                {
                    EntryId = string.Empty,
                    ErrorMessage = "No article id was supplied and none was found in the file."
                });
                continue;
            }

            try
            {
                await ApplyTranslatedFields(targetId, entry);
            }
            catch (Exception ex)
            {
                errors.Add(new ContentProcessingError { EntryId = targetId, ErrorMessage = ex.Message });
            }
        }

        // 4. Return the file stamped with the target-system reference (for Blacklake).
        return await BuildUploadOutput(input, loadResult, parsed, errors);
    }

    /// <summary>Overlays the translated title/body onto the current record and PATCHes only what changed.</summary>
    private async Task ApplyTranslatedFields(string articleId, ParsedEntry entry)
    {
        var current = await Client.GetRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, articleId,
            new Dictionary<string, string> { ["sysparm_fields"] = "sys_id,short_description,text,language" });

        var body = new Dictionary<string, object>();

        foreach (var field in entry.Fields)
        {
            switch (field.FieldId)
            {
                case RoundtripHtml.TitleFieldId:
                    if (field.Value.Trim() != (current.ShortDescription ?? string.Empty).Trim())
                        body["short_description"] = field.Value;
                    break;
                case RoundtripHtml.BodyFieldId:
                    if (ArticleHtmlConverter.NormalizeHtml(field.Value)
                        != ArticleHtmlConverter.NormalizeHtml(current.Text))
                        body["text"] = field.Value;
                    break;
            }
        }

        if (body.Count == 0)
            return; // nothing actually changed — don't touch the API

        await Client.UpdateRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, articleId, body,
            new Dictionary<string, string> { ["sysparm_fields"] = RoundtripFields });
    }

    private async Task<DownloadContentOutput> BuildUploadOutput(
        UploadArticleRequest input, TransformationLoadResult loadResult, ParsedArticleFile parsed,
        List<ContentProcessingError> errors)
    {
        var rootId = !string.IsNullOrWhiteSpace(input.ContentId) ? input.ContentId : parsed.MainEntryId ?? string.Empty;
        var output = new DownloadContentOutput { RootEntryId = rootId };

        if (loadResult.Success)
        {
            var transformation = loadResult.Value;
            await StampTargetReference(transformation, rootId, input.Locale);

            if (loadResult.WasBilingual)
            {
                output.Content = await fileManagementClient.UploadAsync(
                    transformation.ToStream(), RoundtripHtml.Xliff2MediaType, transformation.BilingualFileName);
            }
            else
            {
                var target = transformation.Target();
                if (!target.Success)
                {
                    output.Content = input.Content;
                }
                else
                {
                    var coded = target.Value;
                    coded.SystemReference = transformation.TargetSystemReference;
                    output.Content = await fileManagementClient.UploadAsync(
                        coded.ToStream(MetadataHandling.Include),
                        coded.OriginalMediaType ?? MediaTypeNames.Text.Html,
                        coded.OriginalName ?? input.Content.Name);
                }
            }
        }
        else
        {
            output.Content = input.Content; // not interoperable — echo the input back
        }

        output.Errors = errors.Count > 0 ? errors : null;
        return output;
    }

    /// <summary>Fills the transformation's target-system reference from the article as it now exists.</summary>
    private async Task StampTargetReference(Transformation transformation, string articleId, string locale)
    {
        transformation.TargetLanguage = locale;

        if (string.IsNullOrWhiteSpace(articleId))
            return;

        try
        {
            var article = await Client.GetRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, articleId,
                new Dictionary<string, string> { ["sysparm_fields"] = "sys_id,number,short_description" });

            transformation.TargetSystemReference.ContentId = article.SysId;
            transformation.TargetSystemReference.ContentName = article.ShortDescription ?? string.Empty;
            transformation.TargetSystemReference.AdminUrl = Client.GetArticleAdminUrl(article.SysId);
            transformation.TargetSystemReference.PublicUrl = Client.GetArticlePublicUrl(article.Number);
            transformation.TargetSystemReference.SystemName = RoundtripHtml.SystemName;
            transformation.TargetSystemReference.SystemRef = Client.InstanceBaseUrl.ToString().TrimEnd('/');
        }
        catch
        {
            // best-effort: a missing reference must not fail the whole upload
        }
    }

    [Action("Create article", Description = "Create a new knowledge article with a title and optional HTML body.")]
    public async Task<CreateArticleResponse> CreateArticle([ActionParameter] CreateArticleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Language))
            throw new PluginMisconfigurationException("Please select a value for 'Language'.");
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new PluginMisconfigurationException("Please fill in the 'Title' field.");
        if (string.IsNullOrWhiteSpace(request.KnowledgeBaseId))
            throw new PluginMisconfigurationException("Please select a value for 'Knowledge base ID'.");

        var body = new Dictionary<string, object>
        {
            ["language"] = request.Language,
            ["short_description"] = request.Title,
            ["kb_knowledge_base"] = request.KnowledgeBaseId
        };
        if (request.Content is not null) body["text"] = request.Content;

        var dto = await Client.CreateRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, body,
            new Dictionary<string, string> { ["sysparm_fields"] = "sys_id,number,short_description,workflow_state" });
        return new CreateArticleResponse(dto);
    }

    private static void ValidateArticleId(string articleId)
    {
        if (string.IsNullOrWhiteSpace(articleId))
            throw new PluginMisconfigurationException("Please fill in the 'Article ID' field.");
    }
}
