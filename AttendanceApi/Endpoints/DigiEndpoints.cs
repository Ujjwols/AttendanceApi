using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class DigiEndpoints
{
    public static IEndpointRouteBuilder MapDigiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/test/digi/mode", (DigiMockState state) =>
        {
            return Results.Ok(new
            {
                mode = state.Mode
            });
        });

        app.MapPatch("/api/test/digi/mode", (
            UpdateDigiMockModeRequest request,
            DigiMockState state) =>
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
                "HTTP_500"
            };

            if (!allowedModes.Contains(mode))
            {
                return Results.BadRequest(new
                {
                    message = $"Supported modes: {string.Join(", ", allowedModes)}"
                });
            }

            state.Mode = mode;

            return Results.Ok(new
            {
                mode = state.Mode
            });
        });

        app.MapGet("/api/test/digi/employees", async (
            ExternalAttendanceDbContext db) =>
        {
            var employees = await db.ExternalEmployees
                .OrderBy(x => x.StaffId)
                .ToListAsync();

            return Results.Ok(employees);
        });

        app.MapPost("/api/test/digi/employees", async (
            UpsertExternalEmployeeRequest request,
            ExternalAttendanceDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.StaffId))
            {
                return Results.BadRequest(new
                {
                    message = "staffId is required."
                });
            }

            var staffId = request.StaffId.Trim();

            var employee = await db.ExternalEmployees
                .FirstOrDefaultAsync(x => x.StaffId == staffId);

            if (employee == null)
            {
                employee = new ExternalEmployee
                {
                    StaffId = staffId,
                    FullName = request.FullName,
                    IsActive = request.IsActive ?? true,
                    CreatedOn = DateTimeOffset.Now
                };

                db.ExternalEmployees.Add(employee);
            }
            else
            {
                employee.FullName = request.FullName ?? employee.FullName;
                employee.IsActive = request.IsActive ?? employee.IsActive;
                employee.UpdatedOn = DateTimeOffset.Now;
            }

            await db.SaveChangesAsync();

            return Results.Ok(employee);
        });

        app.MapGet("/api/test/digi/students", async (
            ExternalAttendanceDbContext db) =>
        {
            var students = await db.ExternalStudents
                .OrderBy(x => x.StudentNumber)
                .Select(x => new
                {
                    studentId = x.StudentNumber,
                    x.FullName,
                    x.IsActive,
                    x.CreatedOn,
                    x.UpdatedOn
                })
                .ToListAsync();

            return Results.Ok(students);
        });

        app.MapPost("/api/test/digi/students", async (
            UpsertDigiStudentRequest request,
            ExternalAttendanceDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.StudentId))
            {
                return Results.BadRequest(new
                {
                    message = "studentId is required."
                });
            }

            var studentId = request.StudentId.Trim();

            var student = await db.ExternalStudents
                .FirstOrDefaultAsync(x => x.StudentNumber == studentId);

            if (student == null)
            {
                student = new ExternalStudent
                {
                    StudentNumber = studentId,
                    FullName = request.FullName,
                    IsActive = request.IsActive ?? true,
                    ResponseMode = "ACCEPTED",
                    CreatedOn = DateTimeOffset.Now
                };

                db.ExternalStudents.Add(student);
            }
            else
            {
                student.FullName = request.FullName ?? student.FullName;
                student.IsActive = request.IsActive ?? student.IsActive;
                student.UpdatedOn = DateTimeOffset.Now;
            }

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                studentId = student.StudentNumber,
                student.FullName,
                student.IsActive,
                student.CreatedOn,
                student.UpdatedOn
            });
        });

        app.MapGet("/api/test/digi/events", async (
            ExternalAttendanceDbContext db) =>
        {
            var events = await db.DigiStaffAttendanceEvents
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Results.Ok(events);
        });

        app.MapGet("/api/test/digi/student-events", async (
            ExternalAttendanceDbContext db) =>
        {
            var events = await db.ExternalAttendanceEvents
                .Where(x => x.DeviceType == "DIGI-STUDENT")
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Results.Ok(events);
        });

        // Existing single-record staff endpoint is kept for backward compatibility.
        app.MapPost("/v2/device-attendance/staff", async (
            HttpRequest httpRequest,
            DigiStaffAttendanceRequest request,
            IConfiguration configuration,
            ExternalAttendanceDbContext db,
            DigiMockState state) =>
        {
            var incomingApiKey =
                httpRequest.Headers["api-key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(incomingApiKey))
            {
                return Results.Json(
                    new
                    {
                        message = "Missing API key"
                    },
                    statusCode: 401
                );
            }

            var configuredApiKey =
                configuration["DigiMock:ApiKey"];

            if (string.IsNullOrWhiteSpace(configuredApiKey) ||
                !string.Equals(
                    incomingApiKey,
                    configuredApiKey,
                    StringComparison.Ordinal))
            {
                return Results.Json(
                    new
                    {
                        message = "Invalid API key"
                    },
                    statusCode: 401
                );
            }

            if (state.Mode == "HTTP_500")
            {
                return Results.Json(
                    new
                    {
                        message = "Error recording attendance"
                    },
                    statusCode: 500
                );
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.DeviceNumber) ||
                string.IsNullOrWhiteSpace(request.EmployeeId) ||
                string.IsNullOrWhiteSpace(request.Status) ||
                request.Timestamp == default)
            {
                return Results.Json(
                    new
                    {
                        message =
                            "Make Sure to provide all required fields: deviceNumber, employeeId, status, and timestamp."
                    },
                    statusCode: 401
                );
            }

            if (request.Status != "Check-In" &&
                request.Status != "Check-Out")
            {
                return Results.Json(
                    new
                    {
                        message = "status must be exactly Check-In or Check-Out."
                    },
                    statusCode: 401
                );
            }

            if (request.Timestamp.Offset != TimeSpan.Zero)
            {
                return Results.Json(
                    new
                    {
                        message = "timestamp must be UTC."
                    },
                    statusCode: 401
                );
            }

            var configuredDeviceNumber =
                configuration["DigiMock:DeviceNumber"];

            if (string.IsNullOrWhiteSpace(configuredDeviceNumber) ||
                !string.Equals(
                    request.DeviceNumber,
                    configuredDeviceNumber,
                    StringComparison.Ordinal))
            {
                return Results.Json(
                    new
                    {
                        message = "Device not found"
                    },
                    statusCode: 401
                );
            }

            var employeeId = request.EmployeeId.Trim();

            var employee = await db.ExternalEmployees
                .FirstOrDefaultAsync(x =>
                    x.StaffId == employeeId &&
                    x.IsActive
                );

            if (employee == null)
            {
                return Results.Json(
                    new
                    {
                        message = "User not found"
                    },
                    statusCode: 401
                );
            }

            var dayStart = GetUtcDayStart(request.Timestamp);
            var dayEnd = dayStart.AddDays(1);

            var latestEvent = await db.DigiStaffAttendanceEvents
                .Where(x =>
                    x.EmployeeId == employeeId &&
                    x.Timestamp >= dayStart &&
                    x.Timestamp < dayEnd)
                .OrderByDescending(x => x.Timestamp)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (request.Status == "Check-In" &&
                latestEvent != null &&
                latestEvent.Status == "Check-In")
            {
                return Results.Json(
                    new
                    {
                        message = "Invalid Check-In. User is already In."
                    },
                    statusCode: 401
                );
            }

            if (request.Status == "Check-Out" &&
                latestEvent != null &&
                latestEvent.Status != "Check-In")
            {
                return Results.Json(
                    new
                    {
                        message = "Invalid Check-Out. No prior Check-In found."
                    },
                    statusCode: 401
                );
            }

            db.DigiStaffAttendanceEvents.Add(new DigiStaffAttendanceEvent
            {
                DeviceNumber = request.DeviceNumber,
                EmployeeId = employeeId,
                Status = request.Status,
                Timestamp = request.Timestamp,
                ReceivedAt = DateTimeOffset.Now
            });

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                message = $"{request.Status} recorded successfully!"
            });
        });

        // Digi v3 staff bulk contract.
        app.MapPost("/v2/device-attendance/staff/bulk", async (
            HttpRequest httpRequest,
            DigiStaffBulkAttendanceRequest request,
            IConfiguration configuration,
            ExternalAttendanceDbContext db,
            DigiMockState state) =>
        {
            var incomingApiKey =
                httpRequest.Headers["api-key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(incomingApiKey))
            {
                return Results.Json(new
                {
                    message = "Missing API key"
                }, statusCode: 401);
            }

            var configuredApiKey =
                configuration["DigiMock:ApiKey"];

            if (string.IsNullOrWhiteSpace(configuredApiKey) ||
                !string.Equals(incomingApiKey, configuredApiKey, StringComparison.Ordinal))
            {
                return Results.Json(new
                {
                    message = "Invalid API key"
                }, statusCode: 401);
            }

            if (state.Mode == "HTTP_500")
            {
                return Results.Json(new
                {
                    message = "Error recording bulk attendance"
                }, statusCode: 500);
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.DeviceNumber) ||
                request.AttendanceRecords == null ||
                !request.AttendanceRecords.Any())
            {
                return Results.BadRequest(new
                {
                    message = "Provide deviceNumber and a non-empty attendanceRecords array."
                });
            }

            if (request.AttendanceRecords.Count > 500)
            {
                return Results.BadRequest(new
                {
                    message = "Batch size exceeds the maximum limit of 500 records."
                });
            }

            var invalidRecords = request.AttendanceRecords
                .Select((x, index) => new { record = x, index })
                .Where(x =>
                    x.record == null ||
                    string.IsNullOrWhiteSpace(x.record.EmployeeId) ||
                    string.IsNullOrWhiteSpace(x.record.Status) ||
                    x.record.Timestamp == default)
                .Select(x => new
                {
                    index = x.index,
                    employeeId = x.record?.EmployeeId,
                    status = x.record?.Status,
                    timestamp = x.record?.Timestamp
                })
                .ToList();

            if (invalidRecords.Any())
            {
                return Results.BadRequest(new
                {
                    message = "Each record must include employeeId, status, and timestamp.",
                    invalidRecords
                });
            }

            var configuredDeviceNumber =
                configuration["DigiMock:DeviceNumber"];

            if (string.IsNullOrWhiteSpace(configuredDeviceNumber) ||
                !string.Equals(request.DeviceNumber, configuredDeviceNumber, StringComparison.Ordinal))
            {
                return Results.Json(new
                {
                    message = "Device not found for this institution"
                }, statusCode: 401);
            }

            var successResults = new List<object>();
            var failedResults = new List<object>();

            var orderedRecords = request.AttendanceRecords
                .Select((record, index) => new { record, index })
                .OrderBy(x => x.record.Timestamp)
                .ThenBy(x => x.index)
                .ToList();

            foreach (var item in orderedRecords)
            {
                var record = item.record;
                var employeeId = record.EmployeeId.Trim();

                if (record.Status != "Check-In" &&
                    record.Status != "Check-Out")
                {
                    failedResults.Add(new
                    {
                        employeeId,
                        status = record.Status,
                        timestamp = record.Timestamp,
                        reason = $"Invalid status \"{record.Status}\". Must be \"Check-In\" or \"Check-Out\"."
                    });
                    continue;
                }

                if (record.Timestamp.Offset != TimeSpan.Zero)
                {
                    failedResults.Add(new
                    {
                        employeeId,
                        status = record.Status,
                        timestamp = record.Timestamp,
                        reason = "timestamp must be UTC."
                    });
                    continue;
                }

                var employee = await db.ExternalEmployees
                    .FirstOrDefaultAsync(x =>
                        x.StaffId == employeeId &&
                        x.IsActive);

                if (employee == null)
                {
                    failedResults.Add(new
                    {
                        employeeId,
                        status = record.Status,
                        timestamp = record.Timestamp,
                        reason = "Employee not found"
                    });
                    continue;
                }

                var dayStart = GetUtcDayStart(record.Timestamp);
                var dayEnd = dayStart.AddDays(1);

                var latestEvent = await db.DigiStaffAttendanceEvents
                    .Where(x =>
                        x.EmployeeId == employeeId &&
                        x.Timestamp >= dayStart &&
                        x.Timestamp < dayEnd)
                    .OrderByDescending(x => x.Timestamp)
                    .ThenByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (record.Status == "Check-In" &&
                    latestEvent != null &&
                    latestEvent.Status == "Check-In")
                {
                    failedResults.Add(new
                    {
                        employeeId,
                        status = record.Status,
                        timestamp = record.Timestamp,
                        reason = "Invalid Check-In. User is already checked in."
                    });
                    continue;
                }

                if (record.Status == "Check-Out" &&
                    latestEvent != null &&
                    latestEvent.Status != "Check-In")
                {
                    failedResults.Add(new
                    {
                        employeeId,
                        status = record.Status,
                        timestamp = record.Timestamp,
                        reason = "Invalid Check-Out. No prior Check-In found."
                    });
                    continue;
                }

                db.DigiStaffAttendanceEvents.Add(new DigiStaffAttendanceEvent
                {
                    DeviceNumber = request.DeviceNumber,
                    EmployeeId = employeeId,
                    Status = record.Status,
                    Timestamp = record.Timestamp,
                    ReceivedAt = DateTimeOffset.Now
                });

                // Save each accepted event so later records for the same employee/day
                // in this same bulk request see the updated check-in/check-out state.
                await db.SaveChangesAsync();

                successResults.Add(new
                {
                    employeeId,
                    status = record.Status,
                    timestamp = record.Timestamp
                });
            }

            var total = request.AttendanceRecords.Count;
            var successful = successResults.Count;
            var failed = failedResults.Count;

            return Results.Ok(new
            {
                success = true,
                message = successful == 0
                    ? "No valid records to process."
                    : $"Bulk attendance processed. {successful} succeeded, {failed} failed.",
                summary = new
                {
                    total,
                    successful,
                    failed
                },
                results = new
                {
                    success = successResults,
                    failed = failedResults
                }
            });
        });

        // Digi v3 student bulk contract.
        // This mock intentionally models only the request/response contract and
        // one-time Check-In/Check-Out sequencing. Digi's routine/module engine is external.
        app.MapPost("/v2/device-attendance/student/bulk", async (
            HttpRequest httpRequest,
            DigiStudentBulkAttendanceRequest request,
            IConfiguration configuration,
            ExternalAttendanceDbContext db,
            DigiMockState state) =>
        {
            var incomingApiKey =
                httpRequest.Headers["api-key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(incomingApiKey))
            {
                return Results.Json(new
                {
                    message = "Missing API key"
                }, statusCode: 401);
            }

            var configuredApiKey =
                configuration["DigiMock:ApiKey"];

            if (string.IsNullOrWhiteSpace(configuredApiKey) ||
                !string.Equals(incomingApiKey, configuredApiKey, StringComparison.Ordinal))
            {
                return Results.Json(new
                {
                    message = "Invalid API key"
                }, statusCode: 401);
            }

            if (state.Mode == "HTTP_500")
            {
                return Results.Json(new
                {
                    message = "Error recording bulk attendance"
                }, statusCode: 500);
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.DeviceNumber))
            {
                return Results.BadRequest(new
                {
                    message = "deviceNumber is required."
                });
            }

            if (request.AttendanceRecords == null ||
                !request.AttendanceRecords.Any())
            {
                return Results.BadRequest(new
                {
                    message = "attendanceRecords must be a non-empty array."
                });
            }

            var configuredDeviceNumber =
                configuration["DigiMock:DeviceNumber"];

            if (string.IsNullOrWhiteSpace(configuredDeviceNumber) ||
                !string.Equals(request.DeviceNumber, configuredDeviceNumber, StringComparison.Ordinal))
            {
                return Results.Json(new
                {
                    message = "Device not found."
                }, statusCode: 401);
            }

            var results = new List<object>();
            var validRecordCount = 0;
            var batchRequestId = $"digi-student-bulk-{Guid.NewGuid()}";

            var orderedRecords = request.AttendanceRecords
                .Select((record, index) => new { record, index })
                .OrderBy(x => x.record?.Timestamp ?? DateTimeOffset.MinValue)
                .ThenBy(x => x.index)
                .ToList();

            foreach (var item in orderedRecords)
            {
                var record = item.record;

                if (record == null ||
                    string.IsNullOrWhiteSpace(record.StudentId) ||
                    string.IsNullOrWhiteSpace(record.Status) ||
                    record.Timestamp == default)
                {
                    results.Add(new
                    {
                        index = item.index,
                        studentId = record?.StudentId,
                        status = "failed",
                        message = "Missing required fields: studentId, status, or timestamp."
                    });
                    continue;
                }

                var studentId = record.StudentId.Trim();

                if (record.Status != "Check-In" &&
                    record.Status != "Check-Out")
                {
                    results.Add(new
                    {
                        index = item.index,
                        studentId,
                        status = "failed",
                        message = $"Invalid status \"{record.Status}\". Must be \"Check-In\" or \"Check-Out\"."
                    });
                    continue;
                }

                if (record.Timestamp.Offset != TimeSpan.Zero)
                {
                    results.Add(new
                    {
                        index = item.index,
                        studentId,
                        status = "failed",
                        message = "timestamp must be UTC."
                    });
                    continue;
                }

                validRecordCount++;

                var student = await db.ExternalStudents
                    .FirstOrDefaultAsync(x => x.StudentNumber == studentId);

                if (student == null)
                {
                    results.Add(new
                    {
                        index = item.index,
                        studentId,
                        status = "failed",
                        message = "Student not found."
                    });
                    continue;
                }

                if (!student.IsActive)
                {
                    results.Add(new
                    {
                        index = item.index,
                        studentId,
                        status = "failed",
                        message = "No active batch found for this student."
                    });
                    continue;
                }

                var dayStart = GetUtcDayStart(record.Timestamp);
                var dayEnd = dayStart.AddDays(1);

                var latestEvent = await db.ExternalAttendanceEvents
                    .Where(x =>
                        x.StudentNumber == studentId &&
                        x.DeviceType == "DIGI-STUDENT" &&
                        x.PunchTime >= dayStart &&
                        x.PunchTime < dayEnd)
                    .OrderByDescending(x => x.PunchTime)
                    .ThenByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (record.Status == "Check-In" &&
                    latestEvent != null &&
                    latestEvent.PunchStatus == "Check-In")
                {
                    results.Add(new
                    {
                        index = item.index,
                        studentId,
                        status = "failed",
                        message = "Invalid Check-In. Student is already checked in."
                    });
                    continue;
                }

                if (record.Status == "Check-Out" &&
                    (latestEvent == null || latestEvent.PunchStatus != "Check-In"))
                {
                    results.Add(new
                    {
                        index = item.index,
                        studentId,
                        status = "failed",
                        message = "Invalid Check-Out. No prior Check-In found."
                    });
                    continue;
                }

                db.ExternalAttendanceEvents.Add(new ExternalAttendanceEvent
                {
                    RequestId = batchRequestId,
                    EventId = Guid.NewGuid().ToString(),
                    StudentNumber = studentId,
                    PunchTime = record.Timestamp,
                    PunchStatus = record.Status,
                    DeviceType = "DIGI-STUDENT",
                    DeviceName = request.DeviceNumber,
                    ResultStatus = "ACCEPTED",
                    ResultMessage = "Attendance recorded.",
                    ReceivedAt = DateTimeOffset.Now
                });

                await db.SaveChangesAsync();

                results.Add(new
                {
                    index = item.index,
                    studentId,
                    status = "success",
                    message = "Attendance recorded."
                });
            }

            var failed = results.Count(x =>
            {
                var statusProperty = x.GetType().GetProperty("status");
                return string.Equals(
                    statusProperty?.GetValue(x)?.ToString(),
                    "failed",
                    StringComparison.OrdinalIgnoreCase);
            });

            var successful = results.Count - failed;

            if (validRecordCount == 0)
            {
                return Results.BadRequest(new
                {
                    message = "No valid attendanceRecords to process.",
                    summary = new
                    {
                        total = request.AttendanceRecords.Count,
                        success = 0,
                        failed
                    },
                    results = results.OrderBy(x =>
                    {
                        var indexProperty = x.GetType().GetProperty("index");
                        return (int)(indexProperty?.GetValue(x) ?? 0);
                    }).ToList()
                });
            }

            var orderedResults = results.OrderBy(x =>
            {
                var indexProperty = x.GetType().GetProperty("index");
                return (int)(indexProperty?.GetValue(x) ?? 0);
            }).ToList();

            return Results.Ok(new
            {
                success = true,
                message = $"Bulk attendance processed. {successful} succeeded, {failed} failed.",
                summary = new
                {
                    total = request.AttendanceRecords.Count,
                    success = successful,
                    failed
                },
                results = orderedResults
            });
        });

        return app;
    }

    private static DateTimeOffset GetUtcDayStart(DateTimeOffset timestamp)
    {
        var utc = timestamp.ToUniversalTime();

        return new DateTimeOffset(
            utc.Year,
            utc.Month,
            utc.Day,
            0,
            0,
            0,
            TimeSpan.Zero);
    }

}
