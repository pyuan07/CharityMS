using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CharityMS.Migrations.CharityMSApplicationDb
{
    public partial class AddPropertiesToDonationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Donation",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceiverId",
                table: "Donation",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Donation",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Donation");

            migrationBuilder.DropColumn(
                name: "ReceiverId",
                table: "Donation");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Donation");
        }
    }
}
