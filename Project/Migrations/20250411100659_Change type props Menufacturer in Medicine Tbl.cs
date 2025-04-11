using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class ChangetypepropsMenufacturerinMedicineTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecordDetail_TreatmentTracking_TreatmentTrackingId",
                table: "TreatmentRecordDetail");

            migrationBuilder.AlterColumn<Guid>(
                name: "TreatmentTrackingId",
                table: "TreatmentRecordDetail",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "Manufacturer",
                table: "Medicine",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecordDetail_TreatmentTracking_TreatmentTrackingId",
                table: "TreatmentRecordDetail",
                column: "TreatmentTrackingId",
                principalTable: "TreatmentTracking",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecordDetail_TreatmentTracking_TreatmentTrackingId",
                table: "TreatmentRecordDetail");

            migrationBuilder.AlterColumn<Guid>(
                name: "TreatmentTrackingId",
                table: "TreatmentRecordDetail",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Manufacturer",
                table: "Medicine",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecordDetail_TreatmentTracking_TreatmentTrackingId",
                table: "TreatmentRecordDetail",
                column: "TreatmentTrackingId",
                principalTable: "TreatmentTracking",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
