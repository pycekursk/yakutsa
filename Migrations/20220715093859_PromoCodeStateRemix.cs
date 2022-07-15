using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace yakutsa.Migrations
{
    public partial class PromoCodeStateRemix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PromoCode");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "PromoCode");

            migrationBuilder.AddColumn<int>(
                name: "PromoCodeState",
                table: "PromoCode",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PromoCodeState",
                table: "PromoCode");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PromoCode",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "PromoCode",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
