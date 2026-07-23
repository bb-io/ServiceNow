using System.Net;
using System.Text;
using Apps.ServiceNow.Constants;
using Apps.ServiceNow.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.ServiceNow.Api;

public partial class Client : BlackBirdRestClient
{
    public Uri InstanceBaseUrl { get; }

    public Client(IEnumerable<AuthenticationCredentialsProvider> creds) : base(new RestClientOptions
    {
        BaseUrl = GetBaseUrl(creds),
        Timeout = TimeSpan.FromSeconds(180)
    })
    {
        InstanceBaseUrl = GetBaseUrl(creds);

        var user = creds.Get(CredsNames.Username).Value;
        var pass = creds.Get(CredsNames.Password).Value;
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}"));
        this.AddDefaultHeader("Authorization", $"Basic {token}");
    }

    public string GetArticleAdminUrl(string sysId) =>
        $"{InstanceBaseUrl.ToString().TrimEnd('/')}/kb_knowledge.do?sys_id={sysId}";

    public string? GetArticlePublicUrl(string? number) =>
        string.IsNullOrWhiteSpace(number)
            ? null
            : $"{InstanceBaseUrl.ToString().TrimEnd('/')}/kb_view.do?sysparm_article={number}";

    private static Uri GetBaseUrl(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var raw = creds.Get(CredsNames.InstanceUrl).Value?.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(raw))
            throw new PluginMisconfigurationException(
                "The instance URL is empty. Please fill in the 'Instance URL' field of the connection, for example https://your-instance.service-now.com");

        if (!raw.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            raw = $"https://{raw}";

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
            throw new PluginMisconfigurationException(
                $"The instance URL '{raw}' is not a valid address. Use the form https://your-instance.service-now.com");

        return uri;
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var message = ExtractErrorMessage(response);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            return new PluginMisconfigurationException(
                $"Authentication failed: {message}. Check the username and password in your ServiceNow connection.");

        if (response.StatusCode is HttpStatusCode.NotFound)
            return new PluginMisconfigurationException(
                $"Record not found or access denied: {message}. Check the ID you supplied.");

        if (response.StatusCode is HttpStatusCode.BadRequest)
            return new PluginMisconfigurationException($"Invalid request: {message}. Please review your input.");

        return new PluginApplicationException($"ServiceNow returned an error: {message}");
    }

    private static string ExtractErrorMessage(RestResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Content))
            return response.ErrorMessage ?? response.StatusDescription ?? response.StatusCode.ToString();

        try
        {
            var error = JsonConvert.DeserializeObject<ErrorDto>(response.Content);
            var text = error?.FirstNonEmpty();
            return string.IsNullOrWhiteSpace(text) ? response.Content : text;
        }
        catch (JsonException)
        {
            return response.Content;
        }
    }
}
