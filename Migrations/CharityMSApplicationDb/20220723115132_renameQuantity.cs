using Microsoft.EntityFrameworkCore.Migrations;

namespace CharityMS.Migrations.CharityMSApplicationDb
{
    public partial class renameQuantity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quanlity",
                table: "Item");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Item",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Item");

            migrationBuilder.AddColumn<int>(
                name: "Quanlity",
                table: "Item",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
