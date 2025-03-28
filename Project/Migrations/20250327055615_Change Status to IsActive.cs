using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStatustoIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "MedicineCategory",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Medicine",
                newName: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "MedicineCategory",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Medicine",
                newName: "Status");
        }
    }
}
