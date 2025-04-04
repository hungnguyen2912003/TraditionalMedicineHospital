using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class CreateRelationshipDepartmentandTreatmentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "TreatmentMethod",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentMethod_DepartmentId",
                table: "TreatmentMethod",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentMethod_Department_DepartmentId",
                table: "TreatmentMethod",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentMethod_Department_DepartmentId",
                table: "TreatmentMethod");

            migrationBuilder.DropIndex(
                name: "IX_TreatmentMethod_DepartmentId",
                table: "TreatmentMethod");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "TreatmentMethod");
        }
    }
}
