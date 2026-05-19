using System;
using System.ComponentModel.DataAnnotations;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class UpdateOpenAppDto
{
    [Required]
    [StringLength(OpenPlatformConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    public DateTime? BeginTime { get; set; }

    public DateTime? EndTime { get; set; }

    [StringLength(OpenPlatformConsts.MaxIpRangesLength)]
    public string? AllowedIpRanges { get; set; }

    [StringLength(OpenPlatformConsts.MaxDescriptionLength)]
    public string? Description { get; set; }
}
