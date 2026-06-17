using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBank.Migrations
{
    /// <inheritdoc />
    public partial class UsrSettingForceMyKeyMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ForceMyKey",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForceMyKey",
                table: "UserSettings");
        }
    }
}
