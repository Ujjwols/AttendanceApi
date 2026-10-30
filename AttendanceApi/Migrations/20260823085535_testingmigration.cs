using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceApi.Migrations
{
    /// <inheritdoc />
    public partial class testingmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EasyAttendanceEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AttenDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AttenTime = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NepDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ComId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MachineId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EasyAttendanceEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EasyAttendanceEvents_ReceivedAt",
                table: "EasyAttendanceEvents",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EasyAttendanceEvents_StaffId",
                table: "EasyAttendanceEvents",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EasyAttendanceEvents");
        }
    }
}
