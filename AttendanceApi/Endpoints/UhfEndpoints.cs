using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class UhfEndpoints
{
    public static IEndpointRouteBuilder MapUhfEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/uhf/webhook", async (
            HttpRequest httpRequest,
            UhfWebhookRequest request,
            IConfiguration configuration,
            ExternalAttendanceDbContext db) =>
        {
            var receivedAt = DateTimeOffset.Now;

            var configuredKey = configuration["ExternalAttendance:ApiKey"];

            if (!string.IsNullOrWhiteSpace(configuredKey))
            {
                var incomingKey = httpRequest.Headers["key"].FirstOrDefault();

                if (incomingKey != configuredKey)
                {
                    return Results.Json(new UhfWebhookResponse
                    {
                        Acknowledged = false,
                        AcknowledgementReference = Guid.NewGuid().ToString(),
                        RequestId = request?.RequestId,
                        ReceivedAt = receivedAt,
                        Status = "FAILED",
                        Message = "Unauthorized request.",
                        Results = new List<UhfWebhookResult>()
                    }, statusCode: 401);
                }
            }

            if (request == null || request.Events == null || !request.Events.Any())
            {
                return Results.BadRequest(new UhfWebhookResponse
                {
                    Acknowledged = false,
                    AcknowledgementReference = Guid.NewGuid().ToString(),
                    RequestId = request?.RequestId,
                    ReceivedAt = receivedAt,
                    Status = "FAILED",
                    Message = "No events received.",
                    Results = new List<UhfWebhookResult>()
                });
            }

            var results = new List<UhfWebhookResult>();

            foreach (var attendanceEvent in request.Events)
            {
                var result = await ProcessSingleEvent(
                    request.RequestId,
                    attendanceEvent,
                    receivedAt,
                    db
                );

                results.Add(result);
            }

            var acceptedCount = results.Count(x => x.Status == "ACCEPTED");
            var duplicatedCount = results.Count(x => x.Status == "DUPLICATED");
            var failedCount = results.Count(x => x.Status == "REJECTED");

            var responseStatus = failedCount == 0
                ? "PROCESSED"
                : acceptedCount > 0 || duplicatedCount > 0
                    ? "PARTIAL"
                    : "FAILED";

            return Results.Ok(new UhfWebhookResponse
            {
                Acknowledged = failedCount == 0,
                AcknowledgementReference = Guid.NewGuid().ToString(),
                RequestId = request.RequestId,
                ReceivedAt = receivedAt,
                Status = responseStatus,
                Message = $"{acceptedCount} accepted, {duplicatedCount} duplicated, {failedCount} failed.",
                Results = results
            });
        });

        return app;
    }

    private static async Task<UhfWebhookResult> ProcessSingleEvent(
        string requestId,
        UhfWebhookEvent attendanceEvent,
        DateTimeOffset receivedAt,
        ExternalAttendanceDbContext db)
    {
        if (attendanceEvent == null)
        {
            return new UhfWebhookResult
            {
                Status = "REJECTED",
                Message = "Event payload is required."
            };
        }

        if (string.IsNullOrWhiteSpace(attendanceEvent.EventId))
        {
            return new UhfWebhookResult
            {
                EventId = attendanceEvent.EventId,
                Status = "REJECTED",
                Reference = attendanceEvent.StudentNumber,
                Message = "eventId is required."
            };
        }

        if (string.IsNullOrWhiteSpace(attendanceEvent.StudentNumber))
        {
            return new UhfWebhookResult
            {
                EventId = attendanceEvent.EventId,
                Status = "REJECTED",
                Reference = attendanceEvent.StudentNumber,
                Message = "studentNumber is required."
            };
        }

        if (string.IsNullOrWhiteSpace(attendanceEvent.PunchStatus))
        {
            return new UhfWebhookResult
            {
                EventId = attendanceEvent.EventId,
                Status = "REJECTED",
                Reference = attendanceEvent.StudentNumber,
                Message = "punchStatus is required."
            };
        }

        var existingEvent = await db.ExternalAttendanceEvents
            .FirstOrDefaultAsync(x => x.EventId == attendanceEvent.EventId);

        if (existingEvent != null &&
            string.Equals(existingEvent.ResultStatus, "ACCEPTED", StringComparison.OrdinalIgnoreCase))
        {
            return new UhfWebhookResult
            {
                EventId = attendanceEvent.EventId,
                Status = "DUPLICATED",
                Reference = attendanceEvent.StudentNumber,
                Message = "Duplicate eventId received."
            };
        }

        var student = await db.ExternalStudents
            .FirstOrDefaultAsync(x => x.StudentNumber == attendanceEvent.StudentNumber);

        string resultStatus;
        string resultMessage;

        if (student == null)
        {
            resultStatus = "REJECTED";
            resultMessage = "Student not found in external API.";
        }
        else if (!student.IsActive)
        {
            resultStatus = "REJECTED";
            resultMessage = $"Student is not active: {student.FullName ?? student.StudentNumber}";
        }
        else if (string.Equals(student.ResponseMode, "REJECTED", StringComparison.OrdinalIgnoreCase))
        {
            resultStatus = "REJECTED";
            resultMessage = "Manually rejected for testing.";
        }
        else if (string.Equals(student.ResponseMode, "DUPLICATED", StringComparison.OrdinalIgnoreCase))
        {
            resultStatus = "DUPLICATED";
            resultMessage = "Manually marked duplicated for testing.";
        }
        else
        {
            resultStatus = "ACCEPTED";
            resultMessage = $"Received attendance event for student: {attendanceEvent.StudentNumber}";
        }

        if (existingEvent == null)
        {
            db.ExternalAttendanceEvents.Add(new ExternalAttendanceEvent
            {
                RequestId = requestId,
                EventId = attendanceEvent.EventId,
                StudentNumber = attendanceEvent.StudentNumber,
                PunchTime = attendanceEvent.PunchTime,
                PunchStatus = attendanceEvent.PunchStatus,
                DeviceType = attendanceEvent.DeviceType,
                DeviceName = attendanceEvent.DeviceName,
                ResultStatus = resultStatus,
                ResultMessage = resultMessage,
                ReceivedAt = receivedAt
            });
        }
        else
        {
            existingEvent.RequestId = requestId;
            existingEvent.StudentNumber = attendanceEvent.StudentNumber;
            existingEvent.PunchTime = attendanceEvent.PunchTime;
            existingEvent.PunchStatus = attendanceEvent.PunchStatus;
            existingEvent.DeviceType = attendanceEvent.DeviceType;
            existingEvent.DeviceName = attendanceEvent.DeviceName;
            existingEvent.ResultStatus = resultStatus;
            existingEvent.ResultMessage = resultMessage;
            existingEvent.UpdatedOn = DateTimeOffset.Now;
        }

        await db.SaveChangesAsync();

        return new UhfWebhookResult
        {
            EventId = attendanceEvent.EventId,
            Status = resultStatus,
            Reference = attendanceEvent.StudentNumber,
            Message = resultMessage
        };
    }

}
