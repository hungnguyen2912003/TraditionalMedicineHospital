using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class InitialCategoryandEmployeeTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medicine_MedicineCategory_MedicineCategoryId",
                table: "Medicine");

            migrationBuilder.AddForeignKey(
                name: "FK_Medicine_MedicineCategory_MedicineCategoryId",
                table: "Medicine",
                column: "MedicineCategoryId",
                principalTable: "MedicineCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medicine_MedicineCategory_MedicineCategoryId",
                table: "Medicine");

            migrationBuilder.AddForeignKey(
                name: "FK_Medicine_MedicineCategory_MedicineCategoryId",
                table: "Medicine",
                column: "MedicineCategoryId",
                principalTable: "MedicineCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
