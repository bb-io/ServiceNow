namespace Apps.ServiceNow.Models.Polling;

public class PollingMemory
{
    public DateTime? LastPollingTime { get; set; }
}

public class ArticleStatePollingMemory
{
    public DateTime? LastPollingTime { get; set; }

    public Dictionary<string, string> ArticleStates { get; set; } = new();
}
