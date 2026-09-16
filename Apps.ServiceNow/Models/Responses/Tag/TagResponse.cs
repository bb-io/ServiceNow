using Apps.ServiceNow.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.ServiceNow.Models.Responses.Tag;

public class TagResponse(TagDto dto)
{
    [Display("Tag ID")] 
    public string TagId { get; set; } = dto.SysId;

    [Display("Tag name")]
    public string? TagName { get; set; } = dto.Name;

    [Display("Tag short description")]
    public string? ShortDescription { get; set; } = dto.ShortDescription;

    [Display("Tag is active")]
    public bool TagIsActive { get; set; } = dto.Active;

    [Display("Tag type")]
    public string TagType { get; set; } = dto.Type;

    [Display("Tag color")]
    public string? TagColor { get; set; } = dto.Color;

    [Display("Tag created on")]
    public DateTime CreatedOn { get; set; } = dto.CreatedOn;

    [Display("Tag created by")]
    public string CreatedBy { get; set; } = dto.CreatedBy;

    [Display("Tag updated on")]
    public DateTime UpdatedOn { get; set; } = dto.UpdatedOn;

    [Display("Tag updated by")]
    public string UpdatedBy { get; set; } = dto.UpdatedBy;
}