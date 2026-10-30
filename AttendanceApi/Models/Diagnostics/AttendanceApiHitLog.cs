using System;

public class AttendanceApiHitLog
{
    public string TraceId { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public long DurationMs { get; set; }
    public string Method { get; set; }
    public string Path { get; set; }
    public string RequestContentType { get; set; }
    public string RequestPayloadJson { get; set; }
    public int HttpStatusCode { get; set; }
    public string ResponseContentType { get; set; }
    public string ResponsePayloadJson { get; set; }
    public string ExceptionMessage { get; set; }
    public bool RequestAborted { get; set; }
}
