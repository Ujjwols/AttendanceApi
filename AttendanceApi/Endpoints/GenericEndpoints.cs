using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class GenericEndpoints
{
    public static IEndpointRouteBuilder MapGenericEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/test/generic/custom-bulk", async (
            HttpRequest httpRequest,
            GenericCustomAttendanceRequest request,
            IConfiguration configuration,
            ExternalAttendanceDbContext db,
            GenericCustomMockState state) =>
        {
            var configuredKey = configuration["ExternalAttendance:ApiKey"];
            if (string.IsNullOrWhiteSpace(configuredKey))
                return Results.Json(new { message = "Configure ExternalAttendance:ApiKey for the Generic mock." },
                    statusCode: 500);

            if (!string.Equals(httpRequest.Headers["key"].FirstOrDefault(),
                configuredKey, StringComparison.Ordinal))
                return Results.Json(new { message = "Invalid API key." }, statusCode: 401);

            if (request == null || string.IsNullOrWhiteSpace(request.BatchCode) ||
                request.SentAtUtc == default || request.Punches == null ||
                request.Punches.Count == 0 || request.Punches.Count > 20)
                return Results.BadRequest(new
                {
                    message = "Provide batchCode, sentAtUtc, and 1 to 20 punches."
                });

            var results = new List<GenericCustomAttendanceResult>();
            foreach (var punch in request.Punches)
            {
                var item = new GenericCustomAttendanceResult
                {
                    EventId = punch?.EventKey,
                    Reference = punch == null ? null : $"{punch.PersonType}:{punch.PersonId}"
                };

                string error = null;
                if (punch == null || string.IsNullOrWhiteSpace(punch.EventKey))
                    error = "eventKey is required.";
                else if (punch.PersonType != 1 && punch.PersonType != 2)
                    error = "personType must be 1 (student) or 2 (employee).";
                else if (punch.PersonId <= 0)
                    error = "personId must be positive.";
                else if (punch.Movement != "IN" && punch.Movement != "OUT")
                    error = "movement must be IN or OUT.";
                else if (punch.OccurredAtUtc == default)
                    error = "occurredAtUtc is required.";
                else
                {
                    var exists = punch.PersonType == 1
                        ? await db.ExternalStudents.AsNoTracking()
                            .AnyAsync(x => x.Id == punch.PersonId && x.IsActive)
                        : await db.ExternalEmployees.AsNoTracking()
                            .AnyAsync(x => x.Id == punch.PersonId && x.IsActive);
                    if (!exists)
                        error = "No active student or employee with this personType and personId.";
                }

                if (error != null)
                {
                    item.Status = "REJECTED";
                    item.Message = error;
                }
                else if (state.AcceptedEventIds.TryAdd(
                    punch.EventKey, (punch.PersonType, punch.PersonId)))
                {
                    item.Status = "ACCEPTED";
                    item.Message = "Generic attendance recorded by the mock API.";
                }
                else
                {
                    var original = state.AcceptedEventIds[punch.EventKey];
                    var samePerson = original.PersonType == punch.PersonType &&
                                     original.PersonId == punch.PersonId;
                    item.Status = samePerson ? "DUPLICATED" : "REJECTED";
                    item.Message = samePerson
                        ? "This eventKey was already accepted."
                        : "This eventKey belongs to a different person.";
                }

                results.Add(item);
            }

            var rejected = results.Count(x => x.Status == "REJECTED");
            var status = rejected == 0 ? "PROCESSED"
                : rejected == results.Count ? "FAILED" : "PARTIAL";

            state.Received.Enqueue(new GenericCustomReceivedRequest
            {
                ReceivedAt = DateTimeOffset.UtcNow,
                Payload = request,
                Status = status,
                Results = results
            });
            while (state.Received.Count > 100)
                state.Received.TryDequeue(out _);

            // The sender matches acknowledgements using eventId, regardless of the
            // configured request field name (eventKey in this mock contract).
            return Results.Ok(new
            {
                requestId = request.BatchCode,
                status,
                message = $"{results.Count - rejected} acknowledged, {rejected} rejected.",
                results
            });
        });

        app.MapGet("/api/test/generic/received", (
            HttpRequest httpRequest,
            IConfiguration configuration,
            GenericCustomMockState state) =>
        {
            var configuredKey = configuration["ExternalAttendance:ApiKey"];
            if (string.IsNullOrWhiteSpace(configuredKey))
                return Results.Json(new { message = "Configure ExternalAttendance:ApiKey for the Generic mock." },
                    statusCode: 500);

            if (!string.Equals(httpRequest.Headers["key"].FirstOrDefault(),
                configuredKey, StringComparison.Ordinal))
                return Results.Json(new { message = "Invalid API key." }, statusCode: 401);

            return Results.Ok(state.Received.ToArray().Reverse());
        });

        return app;
    }

}
