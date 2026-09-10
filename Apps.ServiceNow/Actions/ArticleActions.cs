using System.Net.Mime;
using System.Text;
using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Extensions;
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
    private const string MetadataFields = TableFields.Article;
    private const string RoundtripFields = "sys_id,number,short_description,text,language,workflow_state";

    [Action("Search articles", Description = "Find knowledge articles matching a search text and optional filters.")]
    public async Task<SearchArticlesResponse> SearchArticles([ActionParameter] SearchArticlesRequest request)
    {
        if (request.Limit is <= 0)
            throw new PluginMisconfigurationException("The 'Maximum results' value must be greater than zero.");

        var clauses = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Query))
            clauses.Add($"123TEXTQUERY321={request.Query}");
        if (!string.IsNullOrWhiteSpace(request.Language))
            clauses.Add($"language={request.Language}");
        if (!string.IsNullOrWhiteSpace(request.State))
            clauses.Add($"workflow_state={request.State}");

        var kbIds = request.KnowledgeBaseIds?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (kbIds is { Count: > 0 })
            clauses.Add($"kb_knowledge_baseIN{string.Join(",", kbIds)}");

        if (request.CreatedAfter.HasValue)
            clauses.Add($"sys_created_on>={ServiceNowDate.FormatUtc(request.CreatedAfter.Value)}");
        if (request.CreatedBefore.HasValue)
            clauses.Add($"sys_created_on<={ServiceNowDate.FormatUtc(request.CreatedBefore.Value)}");
        if (request.UpdatedAfter.HasValue)
            clauses.Add($"sys_updated_on>={ServiceNowDate.FormatUtc(request.UpdatedAfter.Value)}");
        if (request.UpdatedBefore.HasValue)
            clauses.Add($"sys_updated_on<={ServiceNowDate.FormatUtc(request.UpdatedBefore.Value)}");

        if (string.IsNullOrWhiteSpace(request.Query))
            clauses.Add("ORDERBYDESCsys_updated_on");

        var query = new Dictionary<string, string> { ["sysparm_fields"] = TableFields.ArticleSearch };
        if (clauses.Count > 0)
            query["sysparm_query"] = string.Join("^", clauses);

        var articles = await Client.SearchTableAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, query, request.Limit);
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

        var body = new Dictionary<string, object>()
            .AddIfNotNull("short_description", request.Title)
            .AddIfNotNull("text", request.Body)
            .AddIfNotEmpty("kb_knowledge_base", request.KnowledgeBaseId)
            .AddIfNotEmpty("kb_category", request.CategoryId)
            .AddIfNotEmpty("language", request.Language);

        if (body.Count == 0)
            throw new PluginMisconfigurationException(
                "No changes were provided. Fill in at least one field to update on the article.");

        var dto = await Client.UpdateRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, request.ArticleId, body,
            new Dictionary<string, string> { ["sysparm_fields"] = MetadataFields });
        return new ArticleMetadataResponse(dto);
    }

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

        string? articleBody = MediaHelper.ToAbsoluteUrls(dto.Text, Client.InstanceBaseUrl);
        articleBody = await MediaHelper.InlineImages(articleBody, Client.InstanceBaseUrl, Client.DownloadAttachmentAsync);
        
        var model = new ArticleHtmlModel
        {
            EntryId = dto.SysId,
            Locale = locale,
            Title = dto.ShortDescription,
            Body = articleBody,
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
    [Action("Upload article", Description = "Import a translated file (.html/.xliff/.xlf) and write its title and body onto the knowledge article of the selected language, creating that language variant when it does not exist yet.")]
    public async Task<UploadArticleResponse> UploadArticle([ActionParameter] UploadArticleRequest input)
    {
        if (input.Content is null)
            throw new PluginMisconfigurationException("Please provide a file in the 'File' input");

        var name = input.Content.Name ?? string.Empty;
        if (!name.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
            && !name.EndsWith(".xliff", StringComparison.OrdinalIgnoreCase)
            && !name.EndsWith(".xlf", StringComparison.OrdinalIgnoreCase))
            throw new PluginMisconfigurationException("Only .html, .xliff and .xlf files are supported.");

        var errors = new List<ContentProcessingError>();

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

        var parsed = ArticleHtmlConverter.ParseHtml(content);
        if (parsed.Entries.Count == 0 && !ReferenceEquals(content, rawHtml))
            parsed = ArticleHtmlConverter.ParseHtml(rawHtml);

        var locale = ResolveUploadLocale(input, loadResult);
        await ValidateLocale(locale);

        var writtenIds = new List<string>();

        foreach (var entry in parsed.Entries)
        {
            var anchorId = !string.IsNullOrWhiteSpace(input.ContentId) ? input.ContentId : entry.EntryId;
            if (string.IsNullOrWhiteSpace(anchorId))
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
                writtenIds.Add(await ApplyTranslatedFields(anchorId, entry, locale));
            }
            catch (Exception ex)
            {
                errors.Add(new ContentProcessingError { EntryId = anchorId, ErrorMessage = ex.Message });
            }
        }

        return await BuildUploadOutput(input, loadResult, parsed, writtenIds.FirstOrDefault(), locale, errors);
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
        }.AddIfNotNull("text", request.Content);

        var dto = await Client.CreateRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, body,
            new Dictionary<string, string> { ["sysparm_fields"] = "sys_id,number,short_description,workflow_state" });
        return new CreateArticleResponse(dto);
    }
    
    private static string ResolveUploadLocale(UploadArticleRequest input, TransformationLoadResult loadResult)
    {
        if (!string.IsNullOrWhiteSpace(input.Locale))
            return input.Locale;

        var fromFile = loadResult.Success ? loadResult.Value.TargetLanguage : null;
        return string.IsNullOrWhiteSpace(fromFile) ? string.Empty : fromFile;
    }
    
    private async Task<string> ApplyTranslatedFields(string anchorId, ParsedEntry entry, string locale)
    {
        var anchor = await Client.GetRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, anchorId,
            new Dictionary<string, string> { ["sysparm_fields"] = TableFields.ArticleVariant });

        var title = FieldValue(entry, RoundtripHtml.TitleFieldId);
        var body = MediaHelper.ToRelativeUrls(
            MediaHelper.RestoreImageUrls(FieldValue(entry, RoundtripHtml.BodyFieldId)), 
            Client.InstanceBaseUrl);
        
        if (MediaHelper.ContainsInlinedImages(body))
        {
            InvocationContext.Logger?.LogWarning(
                "The uploaded file contains an embedded image that cannot be matched to a ServiceNow attachment", 
                []);
        }

        var (source, variant) = await ResolveLocaleVariant(anchor, locale);
        if (variant is null)
            return (await CreateLocaleVariant(source, locale, title, body)).SysId;

        var changes = new Dictionary<string, object>();

        if (title is not null && title.Trim() != (variant.ShortDescription ?? string.Empty).Trim())
            changes["short_description"] = title;
        if (body is not null && ArticleHtmlConverter.NormalizeHtml(body)
                             != ArticleHtmlConverter.NormalizeHtml(variant.Text))
            changes["text"] = body;

        if (changes.Count > 0)
            await Client.UpdateRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, variant.SysId, changes,
                new Dictionary<string, string> { ["sysparm_fields"] = RoundtripFields });

        return variant.SysId;
    }
    
    private async Task<(ArticleDto Source, ArticleDto? Variant)> ResolveLocaleVariant(ArticleDto anchor, string locale)
    {
        if (string.IsNullOrWhiteSpace(locale) || IsLocale(anchor.Language, locale))
            return (anchor, anchor);

        var source = anchor;
        var parentId = anchor.Parent?.Value;
        if (!string.IsNullOrWhiteSpace(parentId))
        {
            source = await Client.GetRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, parentId,
                new Dictionary<string, string> { ["sysparm_fields"] = TableFields.ArticleVariant });

            if (IsLocale(source.Language, locale))
                return (source, source);
        }

        var siblings = await Client.SearchTableAsync<ArticleDto>(ApiEndpoints.KnowledgeTable,
            new Dictionary<string, string>
            {
                ["sysparm_query"] = $"parent={source.SysId}^language={locale}^ORDERBYDESCsys_updated_on",
                ["sysparm_fields"] = TableFields.ArticleVariant
            }, 1);

        return (source, siblings.FirstOrDefault());
    }

    private async Task<ArticleDto> CreateLocaleVariant(ArticleDto source, string locale, string? title, string? body)
    {
        var create = new Dictionary<string, object>
            {
                ["language"] = locale,
                ["parent"] = source.SysId,
                ["short_description"] = title ?? source.ShortDescription ?? source.Number
            }
            .AddIfNotNull("text", body ?? source.Text)
            .AddIfNotEmpty("kb_knowledge_base", source.KnowledgeBase?.Value)
            .AddIfNotEmpty("kb_category", source.Category?.Value);

        return await Client.CreateRecordAsync<ArticleDto>(ApiEndpoints.KnowledgeTable, create,
            new Dictionary<string, string> { ["sysparm_fields"] = TableFields.ArticleVariant });
    }

    /// <summary>
    /// ServiceNow silently falls back to the instance default language when given a code that is not active, which
    /// would land the translation on an English record. Reject such a code before anything is written.
    /// </summary>
    private async Task ValidateLocale(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
            return;

        var languages = await Client.SearchTableAsync<LanguageItemDto>(ApiEndpoints.LanguageTable,
            new Dictionary<string, string>
            {
                ["sysparm_query"] = "active=true",
                ["sysparm_fields"] = "id,name"
            });

        if (languages.Any(x => IsLocale(x.Id, locale)))
            return;

        var available = string.Join(", ", languages.Select(x => x.Id).Where(x => !string.IsNullOrWhiteSpace(x)));
        throw new PluginMisconfigurationException(
            $"'{locale}' is not an active language on this ServiceNow instance. Active languages: {available}. " +
            "Activate the language in ServiceNow under System Localization, or pick another one in the 'Language' input.");
    }

    private static bool IsLocale(string? value, string locale) =>
        string.Equals(value, locale, StringComparison.OrdinalIgnoreCase);

    private static string? FieldValue(ParsedEntry entry, string fieldId) =>
        entry.Fields.FirstOrDefault(f => f.FieldId == fieldId)?.Value;

    private async Task<UploadArticleResponse> BuildUploadOutput(
        UploadArticleRequest input, TransformationLoadResult loadResult, ParsedArticleFile parsed,
        string? writtenArticleId, string locale, List<ContentProcessingError> errors)
    {
        var rootId = !string.IsNullOrWhiteSpace(input.ContentId) ? input.ContentId : parsed.MainEntryId ?? string.Empty;
        var output = new UploadArticleResponse
        {
            RootEntryId = rootId,
            TargetEntryId = writtenArticleId ?? string.Empty
        };

        if (loadResult.Success)
        {
            var transformation = loadResult.Value;
            await StampTargetReference(transformation, writtenArticleId ?? rootId, locale);

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
            output.Content = input.Content;
        }

        output.Errors = errors.Count > 0 ? errors : null;
        return output;
    }

    private async Task StampTargetReference(Transformation transformation, string articleId, string locale)
    {
        if (!string.IsNullOrWhiteSpace(locale))
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
        }
    }

    private static void ValidateArticleId(string articleId)
    {
        if (string.IsNullOrWhiteSpace(articleId))
            throw new PluginMisconfigurationException("Please fill in the 'Article ID' field.");
    }
}
