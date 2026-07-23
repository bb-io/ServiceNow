using Apps.ServiceNow.Api;
using Apps.ServiceNow.Models.Polling;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;

namespace Apps.ServiceNow;

public class Invocable : BaseInvocable
{
    protected AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected Client Client { get; }

    public Invocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Client = new(Creds);
    }

    protected static PollingEventResponse<PollingMemory, TResult> Baseline<TResult>() where TResult : class =>
        new()
        {
            FlyBird = false,
            Memory = new PollingMemory { LastPollingTime = DateTime.UtcNow },
            Result = null
        };
}
