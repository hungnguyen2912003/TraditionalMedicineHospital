using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class ChangeforeignkeybetweenTreatmentRecordDetailandTreatmentTrackingTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<Guid>(
                name: "TreatmentRecordDetailId",
                table: "TreatmentTracking",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentTracking_TreatmentRecordDetailId",
                table: "TreatmentTracking",
                column: "TreatmentRecordDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentTracking_TreatmentRecordDetail_TreatmentRecordDetailId",
                table: "TreatmentTracking",
                column: "TreatmentRecordDetailId",
                principalTable: "TreatmentRecordDetail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentTracking_TreatmentRecordDetail_TreatmentRecordDetailId",
                table: "TreatmentTracking");

            migrationBuilder.DropIndex(
                name: "IX_TreatmentTracking_TreatmentRecordDetailId",
                table: "TreatmentTracking");

            migrationBuilder.DropColumn(
                name: "TreatmentRecordDetailId",
                table: "TreatmentTracking");

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
    }
}
