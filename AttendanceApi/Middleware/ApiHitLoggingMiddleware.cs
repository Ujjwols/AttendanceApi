using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>Captures Digi/Easy/Generic and new attendance receiver deliveries before validation.</summary>
public sealed class ApiHitLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiHitLogStore _store;
    private readonly ILogger<ApiHitLoggingMiddleware> _logger;

    private static readonly HashSet<string> AttendanceDeliveryPaths =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "/api/test/generic/custom-bulk",
            "/v2/device-attendance/student/bulk",
            "/v2/device-attendance/staff/bulk",
            "/v2/device-attendance/staff",
            "/easy/api/bulk-atten",
            "/api/attendance/receive"
        };

    public ApiHitLoggingMiddleware(
        RequestDelegate next,
        ApiHitLogStore store,
        ILogger<ApiHitLoggingMiddleware> logger)
    {
        _next = next;
        _store = store;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.TrimEnd('/') ?? string.Empty;
        if (!AttendanceDeliveryPaths.Contains(path))
        {
            await _next(context);
            return;
        }

        var receivedAt = DateTimeOffset.UtcNow;
        var timer = Stopwatch.StartNew();
        var requestPayloadJson = string.Empty;
        string exceptionMessage = null;
        var originalResponseBody = context.Response.Body;
        using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        try
        {
            context.Request.EnableBuffering();
            using (var reader = new StreamReader(
                context.Request.Body, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true, bufferSize: 4096,
                leaveOpen: true))
            {
                requestPayloadJson = await reader.ReadToEndAsync();
            }
            context.Request.Body.Position = 0;

            await _next(context);
        }
        catch (Exception ex)
        {
            exceptionMessage = ex.GetType().Name + ": " + ex.Message;
            if (context.Response.HasStarted || context.RequestAborted.IsCancellationRequested)
                throw;

            // Match the existing malformed-JSON and unexpected-exception behavior.
            responseBuffer.SetLength(0);
            context.Response.Clear();
            var statusCode = ex is BadHttpRequestException badRequest
                ? badRequest.StatusCode
                : StatusCodes.Status500InternalServerError;

            _logger.LogError(ex,
                "Attendance request failed. TraceId: {TraceId}", context.TraceIdentifier);

            await Results.Json(new
            {
                message = statusCode < 500
                    ? ex.Message
                    : "The mock API could not process this request. Check its API hit log.",
                traceId = context.TraceIdentifier
            }, statusCode: statusCode).ExecuteAsync(context);
        }
        finally
        {
            timer.Stop();
            context.Response.Body = originalResponseBody;

            var hit = new AttendanceApiHitLog
            {
                TraceId = context.TraceIdentifier,
                ReceivedAt = receivedAt,
                CompletedAt = DateTimeOffset.UtcNow,
                DurationMs = timer.ElapsedMilliseconds,
                Method = context.Request.Method,
                Path = context.Request.PathBase.ToString() + context.Request.Path.ToString(),
                RequestContentType = context.Request.ContentType,
                RequestPayloadJson = requestPayloadJson,
                HttpStatusCode = context.Response.StatusCode,
                ResponseContentType = context.Response.ContentType,
                ResponsePayloadJson = Encoding.UTF8.GetString(responseBuffer.ToArray()),
                ExceptionMessage = exceptionMessage,
                RequestAborted = context.RequestAborted.IsCancellationRequested
            };

            try
            {
                await _store.AppendAsync(hit);
            }
            catch (Exception logException)
            {
                // Diagnostic logging must not replace the attendance response.
                _logger.LogError(logException,
                    "Could not save API hit log. TraceId: {TraceId}", context.TraceIdentifier);
            }
        }

        responseBuffer.Position = 0;
        await responseBuffer.CopyToAsync(originalResponseBody, context.RequestAborted);
    }
}
