using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBank.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluationRemarksField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "DataMigrationHistories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "DataMigrationHistories");
        }
    }
}
