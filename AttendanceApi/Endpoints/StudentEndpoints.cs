using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class StudentEndpoints
{
    public static IEndpointRouteBuilder MapStudentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/test/students", async (ExternalAttendanceDbContext db) =>
        {
            var students = await db.ExternalStudents
                .OrderBy(x => x.StudentNumber)
                .ToListAsync();

            return Results.Ok(students);
        });

        app.MapPost("/api/test/students", async (
            UpsertStudentRequest request,
            ExternalAttendanceDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.StudentNumber))
            {
                return Results.BadRequest(new
                {
                    message = "studentNumber is required."
                });
            }

            var responseMode = string.IsNullOrWhiteSpace(request.ResponseMode)
                ? "ACCEPTED"
                : request.ResponseMode.ToUpper();

            if (responseMode != "ACCEPTED" &&
                responseMode != "REJECTED" &&
                responseMode != "DUPLICATED")
            {
                return Results.BadRequest(new
                {
                    message = "responseMode must be ACCEPTED, REJECTED, or DUPLICATED."
                });
            }

            var student = await db.ExternalStudents
                .FirstOrDefaultAsync(x => x.StudentNumber == request.StudentNumber);

            if (student == null)
            {
                student = new ExternalStudent
                {
                    StudentNumber = request.StudentNumber,
                    FullName = request.FullName,
                    ClassName = request.ClassName,
                    RollNumber = request.RollNumber,
                    IsActive = request.IsActive ?? true,
                    ResponseMode = responseMode,
                    CreatedOn = DateTimeOffset.Now
                };

                db.ExternalStudents.Add(student);
            }
            else
            {
                student.FullName = request.FullName ?? student.FullName;
                student.ClassName = request.ClassName ?? student.ClassName;
                student.RollNumber = request.RollNumber ?? student.RollNumber;
                student.IsActive = request.IsActive ?? student.IsActive;
                student.ResponseMode = responseMode;
                student.UpdatedOn = DateTimeOffset.Now;
            }

            await db.SaveChangesAsync();

            return Results.Ok(student);
        });

        app.MapPatch("/api/test/students/{studentNumber}/mode", async (
            string studentNumber,
            UpdateStudentModeRequest request,
            ExternalAttendanceDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.ResponseMode))
            {
                return Results.BadRequest(new
                {
                    message = "responseMode is required."
                });
            }

            var responseMode = request.ResponseMode.ToUpper();

            if (responseMode != "ACCEPTED" &&
                responseMode != "REJECTED" &&
                responseMode != "DUPLICATED")
            {
                return Results.BadRequest(new
                {
                    message = "responseMode must be ACCEPTED, REJECTED, or DUPLICATED."
                });
            }

            var student = await db.ExternalStudents
                .FirstOrDefaultAsync(x => x.StudentNumber == studentNumber);

            if (student == null)
            {
                return Results.NotFound(new
                {
                    message = "Student not found."
                });
            }

            student.ResponseMode = responseMode;
            student.UpdatedOn = DateTimeOffset.Now;

            await db.SaveChangesAsync();

            return Results.Ok(student);
        });

        return app;
    }

}
