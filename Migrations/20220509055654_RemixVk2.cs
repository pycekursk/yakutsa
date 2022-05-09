using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace yakutsa.Migrations
{
    public partial class RemixVk2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GroupToken",
                table: "Vk",
                newName: "UserAccessToken");

            migrationBuilder.RenameColumn(
                name: "AccessToken",
                table: "Vk",
                newName: "GroupAccessToken");

            migrationBuilder.AlterColumn<ulong>(
                name: "ApplicationId",
                table: "Vk",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserAccessToken",
                table: "Vk",
                newName: "GroupToken");

            migrationBuilder.RenameColumn(
                name: "GroupAccessToken",
                table: "Vk",
                newName: "AccessToken");

            migrationBuilder.AlterColumn<long>(
                name: "ApplicationId",
                table: "Vk",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");
        }
    }
}
