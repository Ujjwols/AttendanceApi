using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceApi.Migrations
{
    /// <inheritdoc />
    public partial class AddDigiStaffMockSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExternalStudents_StaffId",
                table: "ExternalStudents");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "ExternalStudents");

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "EasyAttendanceEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NepDate",
                table: "EasyAttendanceEvents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MachineId",
                table: "EasyAttendanceEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "DigiStaffAttendanceEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigiStaffAttendanceEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalEmployees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalEmployees", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DigiStaffAttendanceEvents_EmployeeId",
                table: "DigiStaffAttendanceEvents",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DigiStaffAttendanceEvents_ReceivedAt",
                table: "DigiStaffAttendanceEvents",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalEmployees_StaffId",
                table: "ExternalEmployees",
                column: "StaffId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DigiStaffAttendanceEvents");

            migrationBuilder.DropTable(
                name: "ExternalEmployees");

            migrationBuilder.AddColumn<string>(
                name: "StaffId",
                table: "ExternalStudents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "EasyAttendanceEvents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NepDate",
                table: "EasyAttendanceEvents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "MachineId",
                table: "EasyAttendanceEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.UpdateData(
                table: "ExternalStudents",
                keyColumn: "Id",
                keyValue: 1,
                column: "StaffId",
                value: "1");

            migrationBuilder.UpdateData(
                table: "ExternalStudents",
                keyColumn: "Id",
                keyValue: 2,
                column: "StaffId",
                value: "2");

            migrationBuilder.UpdateData(
                table: "ExternalStudents",
                keyColumn: "Id",
                keyValue: 3,
                column: "StaffId",
                value: "3");

            migrationBuilder.UpdateData(
                table: "ExternalStudents",
                keyColumn: "Id",
                keyValue: 4,
                column: "StaffId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalStudents_StaffId",
                table: "ExternalStudents",
                column: "StaffId");
        }
    }
}
