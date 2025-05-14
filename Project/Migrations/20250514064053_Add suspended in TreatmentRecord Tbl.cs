using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class AddsuspendedinTreatmentRecordTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SuspendedBy",
                table: "TreatmentRecord",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedDate",
                table: "TreatmentRecord",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspendedNote",
                table: "TreatmentRecord",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspendedReason",
                table: "TreatmentRecord",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuspendedBy",
                table: "TreatmentRecord");

            migrationBuilder.DropColumn(
                name: "SuspendedDate",
                table: "TreatmentRecord");

            migrationBuilder.DropColumn(
                name: "SuspendedNote",
                table: "TreatmentRecord");

            migrationBuilder.DropColumn(
                name: "SuspendedReason",
                table: "TreatmentRecord");
        }
    }
}
