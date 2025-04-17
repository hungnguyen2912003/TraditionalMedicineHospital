using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class ChangePatientTreatmentRecordDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecord_Patient_PatientId",
                table: "TreatmentRecord");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecord_Patient_PatientId",
                table: "TreatmentRecord",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecord_Patient_PatientId",
                table: "TreatmentRecord");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecord_Patient_PatientId",
                table: "TreatmentRecord",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
