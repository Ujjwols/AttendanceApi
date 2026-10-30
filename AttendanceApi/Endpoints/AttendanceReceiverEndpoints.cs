using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Routing;

/// <summary>School reference management and incoming student attendance punches.</summary>
public static class AttendanceReceiverEndpoints
{
    public static IEndpointRouteBuilder MapAttendanceReceiverEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/attendance/receive", ReceiveAttendanceAsync);
        app.MapGet("/api/test/schools", GetSchoolsAsync);
        app.MapPost("/api/test/schools", UpsertSchoolAsync);
        app.MapGet("/api/test/attendance-events", GetAttendanceEventsAsync);
        return app;
    }

    private static async Task<IResult> ReceiveAttendanceAsync(
        HttpRequest httpRequest,
        JsonElement payload,
        IConfiguration configuration,
        ExternalAttendanceDbContext db,
        CancellationToken cancellationToken)
    {
        var accessError = RequireApiKey(httpRequest, configuration);
        if (accessError is not null) return accessError;

        if (!AttendancePayloadParser.TryGetRecords(payload, out var records,
                out var single, out var payloadError))
            return Results.BadRequest(new { message = payloadError });

        var results = new List<AttendanceReceiveItemResult>();
        var pending = new List<(AttendanceEvent Entity, AttendanceReceiveItemResult Result)>();
        var schoolCache = new Dictionary<string, School?>(StringComparer.Ordinal);

        for (var index = 0; index < records.Count; index++)
        {
            if (!AttendancePayloadParser.TryParse(records[index], out var punch, out var error))
            {
                results.Add(new AttendanceReceiveItemResult
                {
                    Index = index,
                    Status = "REJECTED",
                    Message = error
                });
                continue;
            }

            var schoolCode = punch!.SchoolCode;
            if (!schoolCache.TryGetValue(schoolCode, out var school))
            {
                school = await db.SchoolsList.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.SchoolCode == schoolCode, cancellationToken);
                schoolCache[schoolCode] = school;
            }

            if (school is null)
            {
                results.Add(new AttendanceReceiveItemResult
                {
                    Index = index,
                    SchoolCode = schoolCode,
                    Status = "REJECTED",
                    Message = "Unknown schoolCode. Add the school to SchoolsList first."
                });
                continue;
            }

            // Trust only the SchoolName saved in SchoolsList, never a name supplied in JSON.
            var entity = new AttendanceEvent
            {
                SchoolCode = school.SchoolCode,
                SchoolName = school.SchoolName,
                StudentName = punch.StudentName,
                ClassName = punch.ClassName,
                Classroom = punch.Classroom,
                PunchTime = punch.PunchTime,
                PunchStatus = punch.PunchStatus,
                ReceivedAt = DateTimeOffset.UtcNow
            };
            db.AttendanceEvents.Add(entity);

            var result = new AttendanceReceiveItemResult
            {
                Index = index,
                SchoolCode = school.SchoolCode,
                SchoolName = school.SchoolName,
                Status = "ACCEPTED",
                Message = "Attendance saved."
            };
            results.Add(result);
            pending.Add((entity, result));
        }

        if (pending.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            foreach (var item in pending)
                item.Result.AttendanceEventId = item.Entity.Id;
        }

        if (single)
        {
            var result = results[0];
            return result.Status == "ACCEPTED"
                ? Results.Json(result, statusCode: StatusCodes.Status201Created)
                : Results.BadRequest(result);
        }

        var accepted = pending.Count;
        var rejected = results.Count - accepted;
        var summary = new
        {
            total = records.Count,
            accepted,
            rejected,
            results = results.OrderBy(x => x.Index).ToArray()
        };
        return accepted == 0 ? Results.BadRequest(summary) : Results.Ok(summary);
    }

    private static async Task<IResult> GetSchoolsAsync(
        HttpRequest httpRequest,
        IConfiguration configuration,
        ExternalAttendanceDbContext db,
        CancellationToken cancellationToken)
    {
        var accessError = RequireAdminKey(httpRequest, configuration);
        if (accessError is not null) return accessError;

        var schools = await db.SchoolsList.AsNoTracking()
            .OrderBy(x => x.SchoolCode)
            .ToListAsync(cancellationToken);
        return Results.Ok(schools);
    }

    private static async Task<IResult> UpsertSchoolAsync(
        HttpRequest httpRequest,
        UpsertSchoolRequest request,
        IConfiguration configuration,
        ExternalAttendanceDbContext db,
        CancellationToken cancellationToken)
    {
        var accessError = RequireAdminKey(httpRequest, configuration);
        if (accessError is not null) return accessError;

        var code = request?.SchoolCode?.Trim();
        var name = request?.SchoolName?.Trim();
        if (string.IsNullOrWhiteSpace(code) || code.Length > 50 ||
            string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            return Results.BadRequest(new
            {
                message = "schoolCode (max 50) and schoolName (max 200) are required."
            });
        }

        var school = await db.SchoolsList.FirstOrDefaultAsync(
            x => x.SchoolCode == code, cancellationToken);
        if (school is null)
        {
            school = new School { SchoolCode = code, SchoolName = name };
            db.SchoolsList.Add(school);
        }
        else
        {
            school.SchoolName = name;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(school);
    }

    private static async Task<IResult> GetAttendanceEventsAsync(
        HttpRequest httpRequest,
        IConfiguration configuration,
        ExternalAttendanceDbContext db,
        string? schoolCode,
        int? take,
        CancellationToken cancellationToken)
    {
        var accessError = RequireAdminKey(httpRequest, configuration);
        if (accessError is not null) return accessError;

        var limit = take ?? 50;
        if (limit < 1 || limit > 200)
            return Results.BadRequest(new { message = "take must be between 1 and 200." });

        var query = db.AttendanceEvents.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(schoolCode))
        {
            var code = schoolCode.Trim();
            query = query.Where(x => x.SchoolCode == code);
        }

        var events = await query.OrderByDescending(x => x.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return Results.Ok(events);
    }

    // Separate the school's trusted reference-data administration from the sender's ingestion key.
    private static IResult? RequireAdminKey(HttpRequest request, IConfiguration configuration)
    {
        var expectedKey = configuration["ExternalAttendance:AdminApiKey"];
        if (string.IsNullOrWhiteSpace(expectedKey))
            return Results.Json(new { message = "Configure ExternalAttendance:AdminApiKey for the test administration endpoints." },
                statusCode: StatusCodes.Status500InternalServerError);

        var key = request.Headers["admin-key"].FirstOrDefault();
        if (!string.Equals(key, expectedKey, StringComparison.Ordinal))
            return Results.Json(new { message = "Invalid admin API key." },
                statusCode: StatusCodes.Status401Unauthorized);

        return null;
    }

    private static IResult? RequireApiKey(HttpRequest request, IConfiguration configuration)
    {
        var expectedKey = configuration["ExternalAttendance:ApiKey"];
        if (string.IsNullOrWhiteSpace(expectedKey))
            return Results.Json(new { message = "Configure ExternalAttendance:ApiKey to use the attendance receiver." },
                statusCode: StatusCodes.Status500InternalServerError);

        var key = request.Headers["key"].FirstOrDefault();
        if (!string.Equals(key, expectedKey, StringComparison.Ordinal))
            return Results.Json(new { message = "Invalid API key." },
                statusCode: StatusCodes.Status401Unauthorized);

        return null;
    }
}
