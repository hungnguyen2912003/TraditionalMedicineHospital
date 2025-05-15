using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class ChangerelationshipbetweenPaymentandTreatmentRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payment_TreatmentRecordId",
                table: "Payment");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_TreatmentRecordId",
                table: "Payment",
                column: "TreatmentRecordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payment_TreatmentRecordId",
                table: "Payment");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_TreatmentRecordId",
                table: "Payment",
                column: "TreatmentRecordId");
        }
    }
}
