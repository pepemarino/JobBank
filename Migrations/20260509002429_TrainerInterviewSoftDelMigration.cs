using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBank.Migrations
{
    /// <inheritdoc />
    public partial class TrainerInterviewSoftDelMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Trainings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Interviews",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Interviews");
        }
    }
}
