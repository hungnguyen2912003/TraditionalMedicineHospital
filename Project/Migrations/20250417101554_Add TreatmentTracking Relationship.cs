using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class AddTreatmentTrackingRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TreatmentTrackingId",
                table: "TreatmentRecordDetail",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentRecordDetail_TreatmentTrackingId",
                table: "TreatmentRecordDetail",
                column: "TreatmentTrackingId");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecordDetail_TreatmentTracking_TreatmentTrackingId",
                table: "TreatmentRecordDetail",
                column: "TreatmentTrackingId",
                principalTable: "TreatmentTracking",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecordDetail_TreatmentTracking_TreatmentTrackingId",
                table: "TreatmentRecordDetail");

            migrationBuilder.DropIndex(
                name: "IX_TreatmentRecordDetail_TreatmentTrackingId",
                table: "TreatmentRecordDetail");

            migrationBuilder.DropColumn(
                name: "TreatmentTrackingId",
                table: "TreatmentRecordDetail");
        }
    }
}
