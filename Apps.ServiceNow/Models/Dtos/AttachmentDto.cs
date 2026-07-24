using Newtonsoft.Json;

namespace Apps.ServiceNow.Models.Dtos;

public class AttachmentDto
{
    [JsonProperty("sys_id")] public string SysId { get; set; } = string.Empty;
    [JsonProperty("file_name")] public string? FileName { get; set; }
    [JsonProperty("content_type")] public string? ContentType { get; set; }
    [JsonProperty("size_bytes")] public string? SizeBytes { get; set; }
    [JsonProperty("table_name")] public string? TableName { get; set; }
    [JsonProperty("table_sys_id")] public string? TableSysId { get; set; }
    [JsonProperty("download_link")] public string? DownloadLink { get; set; }
}
