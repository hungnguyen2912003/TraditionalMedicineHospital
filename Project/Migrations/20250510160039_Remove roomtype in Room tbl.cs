using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class RemoveroomtypeinRoomtbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Room_TreatmentMethod_TreatmentMethodId",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "RoomType",
                table: "Room");

            migrationBuilder.AlterColumn<Guid>(
                name: "TreatmentMethodId",
                table: "Room",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Room_TreatmentMethod_TreatmentMethodId",
                table: "Room",
                column: "TreatmentMethodId",
                principalTable: "TreatmentMethod",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Room_TreatmentMethod_TreatmentMethodId",
                table: "Room");

            migrationBuilder.AlterColumn<Guid>(
                name: "TreatmentMethodId",
                table: "Room",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoomType",
                table: "Room",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Room_TreatmentMethod_TreatmentMethodId",
                table: "Room",
                column: "TreatmentMethodId",
                principalTable: "TreatmentMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
