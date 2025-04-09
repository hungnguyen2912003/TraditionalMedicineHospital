using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class InitialTreatmentTrackingTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TreatmentTrackingId",
                table: "TreatmentRecordDetail",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "TreatmentTracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TreatmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentTracking", x => x.Id);
                });

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

            migrationBuilder.DropTable(
                name: "TreatmentTracking");

            migrationBuilder.DropIndex(
                name: "IX_TreatmentRecordDetail_TreatmentTrackingId",
                table: "TreatmentRecordDetail");

            migrationBuilder.DropColumn(
                name: "TreatmentTrackingId",
                table: "TreatmentRecordDetail");
        }
    }
}
