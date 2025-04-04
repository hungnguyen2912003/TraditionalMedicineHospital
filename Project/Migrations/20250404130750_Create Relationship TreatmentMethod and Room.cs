using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class CreateRelationshipTreatmentMethodandRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TreatmentMethodId",
                table: "Room",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Room_TreatmentMethodId",
                table: "Room",
                column: "TreatmentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Room_TreatmentMethod_TreatmentMethodId",
                table: "Room",
                column: "TreatmentMethodId",
                principalTable: "TreatmentMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Room_TreatmentMethod_TreatmentMethodId",
                table: "Room");

            migrationBuilder.DropIndex(
                name: "IX_Room_TreatmentMethodId",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "TreatmentMethodId",
                table: "Room");
        }
    }
}
