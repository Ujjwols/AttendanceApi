using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class EasyEndpoints
{
    public static IEndpointRouteBuilder MapEasyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/test/easy/mode", (EasyMockState state) =>
        {
            return Results.Ok(new
            {
                mode = state.Mode
            });
        });

        app.MapPatch("/api/test/easy/mode", (
            UpdateEasyMockModeRequest request,
            EasyMockState state) =>
        {
            if (string.IsNullOrWhiteSpace(request.Mode))
            {
                return Results.BadRequest(new
                {
                    message = "mode is required."
                });
            }

            var mode = request.Mode.Trim().ToUpperInvariant();

            var allowedModes = new[]
            {
                "SUCCESS",
                "BODY_400",
                "HTTP_500",
                "ATTENDANCE_401_ONCE",
                "TOKEN_HTTP_500"
            };

            if (!allowedModes.Contains(mode))
            {
                return Results.BadRequest(new
                {
                    message = $"Supported modes: {string.Join(", ", allowedModes)}"
                });
            }

            state.Mode = mode;
            state.AttendanceUnauthorizedReturned = false;

            return Results.Ok(new
            {
                mode = state.Mode
            });
        });

        app.MapGet("/api/test/easy/events", async (ExternalAttendanceDbContext db) =>
        {
            var events = await db.EasyAttendanceEvents
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Results.Ok(events);
        });

        app.MapPost("/easy/api/access-token", (
            EasyTokenRequest request,
            IConfiguration configuration,
            EasyMockState state) =>
        {
            if (state.Mode == "TOKEN_HTTP_500")
            {
                return Results.Json(
                    new EasyTokenResponse
                    {
                        StatusCode = 500,
                        Message = "Mock token server error.",
                        Token = null,
                        ExpiryMinutes = 0
                    },
                    statusCode: 500
                );
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.ComID) ||
                string.IsNullOrWhiteSpace(request.Login) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.Ok(new EasyTokenResponse
                {
                    StatusCode = 400,
                    Message = "Parameters cannot be empty",
                    Token = null,
                    ExpiryMinutes = 0
                });
            }

            var configuredComId = configuration["EasyMock:ComID"];
            var configuredLogin = configuration["EasyMock:Login"];
            var configuredPassword = configuration["EasyMock:Password"];

            if (!string.Equals(request.ComID, configuredComId, StringComparison.Ordinal) ||
                !string.Equals(request.Login, configuredLogin, StringComparison.Ordinal) ||
                !string.Equals(request.Password, configuredPassword, StringComparison.Ordinal))
            {
                return Results.Ok(new EasyTokenResponse
                {
                    StatusCode = 400,
                    Message = "Invalid login credentials.",
                    Token = null,
                    ExpiryMinutes = 0
                });
            }

            var token = $"mock-easy-token-{Guid.NewGuid()}";
            state.ActiveToken = token;

            return Results.Ok(new EasyTokenResponse
            {
                StatusCode = 200,
                Message = "Success",
                Token = token,
                ExpiryMinutes = 60
            });
        });

        app.MapPost("/easy/api/bulk-atten", async (
            HttpRequest httpRequest,
            EasyAttendanceRequest request,
            IConfiguration configuration,
            ExternalAttendanceDbContext db,
            EasyMockState state) =>
        {
            var authorization = httpRequest.Headers["Authorization"].FirstOrDefault();

            var expectedAuthorization = string.IsNullOrWhiteSpace(state.ActiveToken)
                ? null
                : $"Bearer {state.ActiveToken}";

            if (string.IsNullOrWhiteSpace(expectedAuthorization) ||
                !string.Equals(authorization, expectedAuthorization, StringComparison.Ordinal))
            {
                return Results.Json(
                    new EasyAttendanceResponse
                    {
                        StatusCode = 401,
                        Message = "Unauthorized"
                    },
                    statusCode: 401
                );
            }

            // Tests the main application's cached-token refresh path.
            if (state.Mode == "ATTENDANCE_401_ONCE" &&
                !state.AttendanceUnauthorizedReturned)
            {
                state.AttendanceUnauthorizedReturned = true;

                return Results.Json(
                    new EasyAttendanceResponse
                    {
                        StatusCode = 401,
                        Message = "Mock JWT expired."
                    },
                    statusCode: 401
                );
            }

            if (state.Mode == "HTTP_500")
            {
                return Results.Json(
                    new EasyAttendanceResponse
                    {
                        StatusCode = 500,
                        Message = "Mock Easy attendance server error."
                    },
                    statusCode: 500
                );
            }

            if (request == null || request.Param == null || !request.Param.Any())
            {
                return Results.Ok(new EasyAttendanceResponse
                {
                    StatusCode = 400,
                    Message = "Attendance data is required."
                });
            }

            var configuredComId = configuration["EasyMock:ComID"];

            if (!string.Equals(request.ComID, configuredComId, StringComparison.Ordinal))
            {
                return Results.Ok(new EasyAttendanceResponse
                {
                    StatusCode = 201,
                    Message = "School not found"
                });
            }

            if (state.Mode == "BODY_400")
            {
                return Results.Ok(new EasyAttendanceResponse
                {
                    StatusCode = 400,
                    Message = "No valid attendance records found."
                });
            }

            var receivedAt = DateTimeOffset.Now;

            foreach (var attendance in request.Param)
            {
                db.EasyAttendanceEvents.Add(new EasyAttendanceEvent
                {
                    StaffId = attendance.StaffID,
                    AttenDate = attendance.AttenDate,
                    AttenTime = attendance.AttenTime,
                    NepDate = attendance.NepDate,
                    ComId = request.ComID,
                    Remarks = request.Remarks,
                    MachineId = request.MachineID,
                    ReceivedAt = receivedAt
                });
            }

            await db.SaveChangesAsync();

            return Results.Ok(new EasyAttendanceResponse
            {
                StatusCode = 200,
                Message = "Success",
                AttenLst = request.Param
                    .Select(x => new EasyAttendanceResponseItem
                    {
                        StatusCode = 200,
                        Message = null,
                        NotificationToken = "mock-notification-token",
                        Title = "Device Attendance Alert !",
                        Description = $"Attendance recorded successfully for StaffID {x.StaffID}",
                        Source = "Mock API"
                    })
                    .ToList()
            });
        });

        return app;
    }

}
