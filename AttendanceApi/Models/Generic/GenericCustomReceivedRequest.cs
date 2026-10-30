using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class GenericCustomReceivedRequest
{
    public DateTimeOffset ReceivedAt { get; set; }
    public GenericCustomAttendanceRequest Payload { get; set; }
    public string Status { get; set; }
    public List<GenericCustomAttendanceResult> Results { get; set; }
}
