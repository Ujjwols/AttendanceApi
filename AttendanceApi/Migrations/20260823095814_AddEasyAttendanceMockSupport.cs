using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEasyAttendanceMockSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StaffId",
                table: "ExternalStudents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExternalStudents_StaffId",
                table: "ExternalStudents");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "ExternalStudents");
        }
    }
}
