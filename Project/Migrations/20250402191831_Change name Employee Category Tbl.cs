using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class ChangenameEmployeeCategoryTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_employeeCategories",
                table: "employeeCategories");

            migrationBuilder.RenameTable(
                name: "employeeCategories",
                newName: "EmployeeCategory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeCategory",
                table: "EmployeeCategory",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeCategory",
                table: "EmployeeCategory");

            migrationBuilder.RenameTable(
                name: "EmployeeCategory",
                newName: "employeeCategories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employeeCategories",
                table: "employeeCategories",
                column: "Id");
        }
    }
}
