using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBank.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesInterview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MigrationName",
                table: "DataMigrationHistories",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_IsMigrated",
                table: "Interviews",
                column: "IsMigrated");

            migrationBuilder.CreateIndex(
                name: "IX_DataMigrationHistories_MigrationName",
                table: "DataMigrationHistories",
                column: "MigrationName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Interviews_IsMigrated",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_DataMigrationHistories_MigrationName",
                table: "DataMigrationHistories");

            migrationBuilder.AlterColumn<string>(
                name: "MigrationName",
                table: "DataMigrationHistories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
