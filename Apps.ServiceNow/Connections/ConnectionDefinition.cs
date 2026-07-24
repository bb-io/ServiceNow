using Apps.ServiceNow.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.ServiceNow.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>
    {
        new()
        {
            Name = "Basic authentication",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties = new List<ConnectionProperty>
            {
                new(CredsNames.InstanceUrl)
                {
                    DisplayName = "Instance URL",
                    Description = "The base address of your ServiceNow instance, for example https://your-instance.service-now.com"
                },
                new(CredsNames.Username) { DisplayName = "Username" },
                new(CredsNames.Password) { DisplayName = "Password", Sensitive = true }
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values) =>
        values.Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value)).ToList();
}
