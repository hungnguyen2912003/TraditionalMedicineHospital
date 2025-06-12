using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class ChangerelationshipAdvancePaymentandTreatmentReciord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdvancePayment_TreatmentRecordId",
                table: "AdvancePayment");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancePayment_TreatmentRecordId",
                table: "AdvancePayment",
                column: "TreatmentRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdvancePayment_TreatmentRecordId",
                table: "AdvancePayment");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancePayment_TreatmentRecordId",
                table: "AdvancePayment",
                column: "TreatmentRecordId",
                unique: true);
        }
    }
}
