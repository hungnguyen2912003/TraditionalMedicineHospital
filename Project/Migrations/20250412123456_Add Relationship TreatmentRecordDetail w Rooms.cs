using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationshipTreatmentRecordDetailwRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "TreatmentRecordDetail",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentRecordDetail_RoomId",
                table: "TreatmentRecordDetail",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentRecordDetail_Room_RoomId",
                table: "TreatmentRecordDetail",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentRecordDetail_Room_RoomId",
                table: "TreatmentRecordDetail");

            migrationBuilder.DropIndex(
                name: "IX_TreatmentRecordDetail_RoomId",
                table: "TreatmentRecordDetail");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "TreatmentRecordDetail");
        }
    }
}
