using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class CreaterelationshipTreatmentRecordDetailwRoomtbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "TreatmentRecordDetail",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "TreatmentRecordDetail",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "TreatmentRecord_Regulation",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "HealthInsurance",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Assignment",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

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
                name: "Code",
                table: "TreatmentRecordDetail");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "TreatmentRecordDetail");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "TreatmentRecord_Regulation");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "HealthInsurance");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Assignment");
        }
    }
}
