using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace yakutsa.Migrations
{
    public partial class aga237 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentsJson",
                table: "Vk");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommentsJson",
                table: "Vk",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
