using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class CreateRelationshipforEmployeeandCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeCategoryId",
                table: "Employee",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Employee_EmployeeCategoryId",
                table: "Employee",
                column: "EmployeeCategoryId");

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

            migrationBuilder.DropIndex(
                name: "IX_Employee_EmployeeCategoryId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "EmployeeCategoryId",
                table: "Employee");
        }
    }
}
