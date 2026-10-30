using System;
using System.Collections.Concurrent;

public class EasyMockState
{
    public string Mode { get; set; } = "SUCCESS";

    public string ActiveToken { get; set; }

    public bool AttendanceUnauthorizedReturned { get; set; }
}
