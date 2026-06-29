using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBank.Migrations
{
    /// <inheritdoc />
    public partial class JobCacheAnalysisMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDonated",
                table: "JobAnalysisCache",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegacy",
                table: "JobAnalysisCache",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "JobAnalysisCache",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SourceModelTier",
                table: "JobAnalysisCache",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE JobAnalysisCache SET " +
                "IsLegacy = 1, " +
                "IsPublic = 1, " +
                "IsDonated = 0, SourceModelTier = 'Legacy' " +
                "WHERE UserId IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDonated",
                table: "JobAnalysisCache");

            migrationBuilder.DropColumn(
                name: "IsLegacy",
                table: "JobAnalysisCache");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "JobAnalysisCache");

            migrationBuilder.DropColumn(
                name: "SourceModelTier",
                table: "JobAnalysisCache");
        }
    }
}
