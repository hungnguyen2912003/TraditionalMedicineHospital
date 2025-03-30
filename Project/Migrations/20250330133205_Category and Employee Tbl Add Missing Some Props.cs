using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class CategoryandEmployeeTblAddMissingSomeProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmployeeCategoryCode",
                table: "EmployeeCategory",
                newName: "Code");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "employees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EmployeeCategory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "EmployeeCategory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EmployeeCategory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "EmployeeCategory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "EmployeeCategory",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EmployeeCategory");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "EmployeeCategory");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EmployeeCategory");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "EmployeeCategory");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "EmployeeCategory");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "EmployeeCategory",
                newName: "EmployeeCategoryCode");
        }
    }
}
