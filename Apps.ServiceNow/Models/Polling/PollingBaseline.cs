using Blackbird.Applications.Sdk.Common.Polling;

namespace Apps.ServiceNow.Models.Polling;

/// <summary>
/// Builds the "first run" polling response: no bird flies, and the memory is seeded with the current
/// time so the next poll only reports changes that happen after this baseline was established.
/// </summary>
public static class PollingBaseline
{
    public static PollingEventResponse<PollingMemory, TResult> Create<TResult>() where TResult : class =>
        new()
        {
            FlyBird = false,
            Memory = new PollingMemory { LastPollingTime = DateTime.UtcNow },
            Result = null
        };
}
