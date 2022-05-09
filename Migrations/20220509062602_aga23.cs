using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace yakutsa.Migrations
{
    public partial class aga23 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Login",
                table: "Vk",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Vk",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Login",
                table: "Vk");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Vk");
        }
    }
}
