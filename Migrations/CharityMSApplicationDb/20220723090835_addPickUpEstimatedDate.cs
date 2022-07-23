using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CharityMS.Migrations.CharityMSApplicationDb
{
    public partial class addPickUpEstimatedDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedPickUpDate",
                table: "PickUp",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedPickUpDate",
                table: "PickUp");
        }
    }
}
