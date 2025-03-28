using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDosageMedicine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DosageQuantity",
                table: "Medicine");

            migrationBuilder.DropColumn(
                name: "DosageUnit",
                table: "Medicine");

            migrationBuilder.DropColumn(
                name: "InStock",
                table: "Medicine");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "Medicine");

            migrationBuilder.RenameColumn(
                name: "UnitQuantity",
                table: "Medicine",
                newName: "StockQuantity");

            migrationBuilder.AddColumn<string>(
                name: "StockUnit",
                table: "Medicine",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockUnit",
                table: "Medicine");

            migrationBuilder.RenameColumn(
                name: "StockQuantity",
                table: "Medicine",
                newName: "UnitQuantity");

            migrationBuilder.AddColumn<decimal>(
                name: "DosageQuantity",
                table: "Medicine",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DosageUnit",
                table: "Medicine",
                type: "int",
                maxLength: 10,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InStock",
                table: "Medicine",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitType",
                table: "Medicine",
                type: "int",
                maxLength: 10,
                nullable: false,
                defaultValue: 0);
        }
    }
}
