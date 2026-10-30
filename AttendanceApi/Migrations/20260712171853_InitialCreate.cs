using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AttendanceApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalAttendanceEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StudentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PunchTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PunchStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DeviceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ResultStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResultMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalAttendanceEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalStudents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RollNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ResponseMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalStudents", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ExternalStudents",
                columns: new[] { "Id", "ClassName", "CreatedOn", "FullName", "IsActive", "ResponseMode", "RollNumber", "StudentNumber", "UpdatedOn" },
                values: new object[,]
                {
                    { 1, "AVM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test Student One", true, "ACCEPTED", "01", "AA-82-1", null },
                    { 2, "AVM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test Student Two", true, "REJECTED", "02", "AS-82-2", null },
                    { 3, "AVM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test Student Three", true, "DUPLICATED", "03", "AH-82-3", null },
                    { 4, "Class One", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Test Student Four", false, "REJECTED", "04", "AD-82-4", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalAttendanceEvents_EventId",
                table: "ExternalAttendanceEvents",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalAttendanceEvents_ResultStatus",
                table: "ExternalAttendanceEvents",
                column: "ResultStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalAttendanceEvents_StudentNumber",
                table: "ExternalAttendanceEvents",
                column: "StudentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalStudents_StudentNumber",
                table: "ExternalStudents",
                column: "StudentNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalAttendanceEvents");

            migrationBuilder.DropTable(
                name: "ExternalStudents");
        }
    }
}
