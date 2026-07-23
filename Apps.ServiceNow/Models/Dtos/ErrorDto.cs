using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

public class ErrorDto
{
    [JsonProperty("error")] public ErrorBody? Error { get; set; }
    [JsonProperty("status")] public string? Status { get; set; }

    public string? FirstNonEmpty()
    {
        if (Error == null) return null;
        return !string.IsNullOrWhiteSpace(Error.Message) ? Error.Message : Error.Detail;
    }

    public class ErrorBody
    {
        [JsonProperty("message")] public string? Message { get; set; }
        [JsonProperty("detail")] public string? Detail { get; set; }
    }
}
