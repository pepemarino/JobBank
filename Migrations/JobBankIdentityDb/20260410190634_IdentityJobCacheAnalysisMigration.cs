using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBank.Migrations.JobBankIdentityDb
{
    /// <inheritdoc />
    public partial class IdentityJobCacheAnalysisMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ForceMyKeyy",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForceMyKeyy",
                table: "AspNetUsers");
        }
    }
}
