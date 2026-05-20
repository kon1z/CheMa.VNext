using System;
using System.Collections.Generic;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenPlatformAlarmListDto
{
    public int TotalCount { get; set; }

    public IReadOnlyList<OpenPlatformAlarmDto> Items { get; set; } = [];
}

public class OpenPlatformAlarmDto
{
    public string AlarmId { get; set; } = default!;

    public string AlarmType { get; set; } = default!;

    public string AlarmLevel { get; set; } = default!;

    public string AlarmContent { get; set; } = default!;

    public DateTime? AlarmTime { get; set; }

    public string? Status { get; set; }
}
