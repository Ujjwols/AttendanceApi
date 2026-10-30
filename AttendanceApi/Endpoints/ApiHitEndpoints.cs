using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public static class ApiHitEndpoints
{
    public static IEndpointRouteBuilder MapApiHitEndpoints(this IEndpointRouteBuilder app)
    {
        // Read completed attendance hits without access to SchoolManagement's database.
        // Query options: date=yyyy-MM-dd (UTC), take=1..200, statusCode=100..599.
        app.MapGet("/api/test/api-hits", async (
            HttpRequest httpRequest,
            IConfiguration configuration,
            string? date,
            int? take,
            int? statusCode,
            ApiHitLogStore store,
            ILoggerFactory loggerFactory) =>
        {
            var configuredKey = configuration["ExternalAttendance:ApiKey"];
            if (string.IsNullOrWhiteSpace(configuredKey))
                return Results.Json(new { message = "Configure ExternalAttendance:ApiKey to read API hits." },
                    statusCode: 500);

            if (!string.Equals(httpRequest.Headers["key"].FirstOrDefault(),
                configuredKey, StringComparison.Ordinal))
                return Results.Json(new { message = "Invalid API key." }, statusCode: 401);

            var selectedDateText = date
                ?? DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (!DateTime.TryParseExact(selectedDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var selectedDate))
                return Results.BadRequest(new { message = "date must use yyyy-MM-dd (UTC)." });

            var limit = take ?? 20;
            if (limit < 1 || limit > 200)
                return Results.BadRequest(new { message = "take must be between 1 and 200." });

            if (statusCode.HasValue && (statusCode.Value < 100 || statusCode.Value > 599))
                return Results.BadRequest(new { message = "statusCode must be between 100 and 599." });

            try
            {
                var hits = await store.ReadHitsAsync(selectedDate, limit, statusCode);
                return Results.Ok(new
                {
                    date = selectedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    timeZone = "UTC",
                    count = hits.Length,
                    hits
                });
            }
            catch (System.IO.IOException ex)
            {
                loggerFactory.CreateLogger("ApiHitEndpoints")
                    .LogError(ex, "Could not read API hit log for {Date}.", selectedDateText);
                return Results.Json(new { message = "Could not read the API hit log file." }, statusCode: 500);
            }
            catch (UnauthorizedAccessException ex)
            {
                loggerFactory.CreateLogger("ApiHitEndpoints")
                    .LogError(ex, "Could not read API hit log for {Date}.", selectedDateText);
                return Results.Json(new { message = "The API process cannot read its hit log folder." }, statusCode: 500);
            }
        });

        return app;
    }
}
