using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class Removeadvancedtbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvancePayment");

            migrationBuilder.AddColumn<decimal>(
                name: "AdvancePayment",
                table: "TreatmentRecord",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdvancePayment",
                table: "TreatmentRecord");

            migrationBuilder.CreateTable(
                name: "AdvancePayment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancePayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancePayment_TreatmentRecord_TreatmentRecordId",
                        column: x => x.TreatmentRecordId,
                        principalTable: "TreatmentRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvancePayment_TreatmentRecordId",
                table: "AdvancePayment",
                column: "TreatmentRecordId");
        }
    }
}
