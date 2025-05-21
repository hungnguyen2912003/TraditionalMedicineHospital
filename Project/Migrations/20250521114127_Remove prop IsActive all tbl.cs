using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class RemovepropIsActivealltbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "User_Permission");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TreatmentTracking");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TreatmentRecordDetail");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TreatmentRecord_Regulation");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TreatmentRecord");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TreatmentMethod");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Regulation");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PrescriptionDetail");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Prescription");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MedicineCategory");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Medicine");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "HealthInsurance");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EmployeeCategory");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Assignment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "User_Permission",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TreatmentTracking",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TreatmentRecordDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TreatmentRecord_Regulation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TreatmentRecord",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TreatmentMethod",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Room",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Regulation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PrescriptionDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Prescription",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Permission",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Payment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Patient",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MedicineCategory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Medicine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "HealthInsurance",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EmployeeCategory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Employee",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Department",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Assignment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
