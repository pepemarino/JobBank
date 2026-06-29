using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBank.Migrations
{
    /// <inheritdoc />
    public partial class JobCacheAnalysisIsValidMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobAnalysisCache_Hash",
                table: "JobAnalysisCache");

            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "JobAnalysisCache",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Richness",
                table: "JobAnalysisCache",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "JobAnalysisCache",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobAnalysisCache_Hash",
                table: "JobAnalysisCache",
                column: "Hash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobAnalysisCache_Hash",
                table: "JobAnalysisCache");

            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "JobAnalysisCache");

            migrationBuilder.DropColumn(
                name: "Richness",
                table: "JobAnalysisCache");

            migrationBuilder.DropColumn(
                name: "State",
                table: "JobAnalysisCache");

            migrationBuilder.CreateIndex(
                name: "IX_JobAnalysisCache_Hash",
                table: "JobAnalysisCache",
                column: "Hash",
                unique: true,
                filter: "[Hash] IS NOT NULL");
        }
    }
}
