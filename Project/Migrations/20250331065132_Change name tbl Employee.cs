using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class ChangenametblEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employees_EmployeeCategory_EmployeeCategoryId",
                table: "employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employees",
                table: "employees");

            migrationBuilder.RenameTable(
                name: "employees",
                newName: "Employee");

            migrationBuilder.RenameIndex(
                name: "IX_employees_EmployeeCategoryId",
                table: "Employee",
                newName: "IX_Employee_EmployeeCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employee",
                table: "Employee",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_EmployeeCategory_EmployeeCategoryId",
                table: "Employee",
                column: "EmployeeCategoryId",
                principalTable: "EmployeeCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_EmployeeCategory_EmployeeCategoryId",
                table: "Employee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employee",
                table: "Employee");

            migrationBuilder.RenameTable(
                name: "Employee",
                newName: "employees");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_EmployeeCategoryId",
                table: "employees",
                newName: "IX_employees_EmployeeCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employees",
                table: "employees",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_employees_EmployeeCategory_EmployeeCategoryId",
                table: "employees",
                column: "EmployeeCategoryId",
                principalTable: "EmployeeCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
