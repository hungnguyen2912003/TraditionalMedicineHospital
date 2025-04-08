using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class ModifyRelationshipUserandPatientEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Employee_EmployeeId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Patient_PatientId",
                table: "User");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Employee_EmployeeId",
                table: "User",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Patient_PatientId",
                table: "User",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Employee_EmployeeId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Patient_PatientId",
                table: "User");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Employee_EmployeeId",
                table: "User",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Patient_PatientId",
                table: "User",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id");
        }
    }
}
