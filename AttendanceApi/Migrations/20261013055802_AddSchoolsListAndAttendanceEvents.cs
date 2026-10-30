using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolsListAndAttendanceEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchoolsList",
                columns: table => new
                {
                    SchoolCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolsList", x => x.SchoolCode);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Class = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Classroom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PunchTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PunchStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceEvents_SchoolsList_SchoolCode",
                        column: x => x.SchoolCode,
                        principalTable: "SchoolsList",
                        principalColumn: "SchoolCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceEvents_SchoolCode_PunchTime",
                table: "AttendanceEvents",
                columns: new[] { "SchoolCode", "PunchTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceEvents");

            migrationBuilder.DropTable(
                name: "SchoolsList");
        }
    }
}
