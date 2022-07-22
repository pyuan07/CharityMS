using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CharityMS.Migrations.CharityMSApplicationDb
{
    public partial class AddDonationAndPickUpTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Donation",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    StaffId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PickUp",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DonorId = table.Column<Guid>(nullable: false),
                    StaffId = table.Column<Guid>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    PickUpDate = table.Column<DateTime>(nullable: false),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickUp", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ItemName = table.Column<string>(nullable: true),
                    Quanlity = table.Column<int>(nullable: false),
                    DonationId = table.Column<Guid>(nullable: true),
                    PickUpId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Item_Donation_DonationId",
                        column: x => x.DonationId,
                        principalTable: "Donation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Item_PickUp_PickUpId",
                        column: x => x.PickUpId,
                        principalTable: "PickUp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Item_DonationId",
                table: "Item",
                column: "DonationId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_PickUpId",
                table: "Item",
                column: "PickUpId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "Donation");

            migrationBuilder.DropTable(
                name: "PickUp");
        }
    }
}
