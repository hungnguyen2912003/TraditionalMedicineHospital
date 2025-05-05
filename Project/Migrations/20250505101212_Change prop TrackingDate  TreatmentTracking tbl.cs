using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class ChangepropTrackingDateTreatmentTrackingtbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TreatmentDate",
                table: "TreatmentTracking",
                newName: "TrackingDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TrackingDate",
                table: "TreatmentTracking",
                newName: "TreatmentDate");
        }
    }
}
