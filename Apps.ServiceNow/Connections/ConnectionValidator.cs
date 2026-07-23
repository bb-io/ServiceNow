using System.Net;
using Apps.ServiceNow.Api;
using Apps.ServiceNow.Constants;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.ServiceNow.Connections;

public class ConnectionValidator(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = new Client(authenticationCredentialsProviders.ToArray());
            var request = new RestRequest(ApiEndpoints.KnowledgeArticles, Method.Get)
                .AddQueryParameter("limit", "1");

            var response = await client.ExecuteAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                return new ConnectionValidationResponse
                {
                    IsValid = false,
                    Message = "Authentication failed — check your username and password."
                };

            // The request never reached a ServiceNow server (bad host / DNS / connection refused).
            // A wrong instance URL is a user-fixable configuration problem, so mark it invalid.
            if (response.ResponseStatus != ResponseStatus.Completed)
                return new ConnectionValidationResponse
                {
                    IsValid = false,
                    Message = "Could not reach the instance — check the instance URL. " +
                              (response.ErrorMessage ?? string.Empty)
                };

            // Any other completed-but-non-success response (5xx, transient) is not a credentials
            // problem, so the connection is not marked invalid.
            return new ConnectionValidationResponse { IsValid = true, Message = "Success" };
        }
        catch (Exception ex)
        {
            InvocationContext.Logger?.LogError($"[ServiceNow] Connection validation failed: {ex.Message}", []);
            return new ConnectionValidationResponse { IsValid = false, Message = ex.Message };
        }
    }
}
