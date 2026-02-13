using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBank.Migrations
{
    /// <inheritdoc />
    public partial class JobPostUserSkillAnalysisNavMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobDescription",
                table: "UserSkillMatchReport",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "JobPostId",
                table: "UserSkillMatchReport",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserSkillMatchReport_JobPostId",
                table: "UserSkillMatchReport",
                column: "JobPostId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSkillMatchReport_JobPost_JobPostId",
                table: "UserSkillMatchReport",
                column: "JobPostId",
                principalTable: "JobPost",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSkillMatchReport_JobPost_JobPostId",
                table: "UserSkillMatchReport");

            migrationBuilder.DropIndex(
                name: "IX_UserSkillMatchReport_JobPostId",
                table: "UserSkillMatchReport");

            migrationBuilder.DropColumn(
                name: "JobDescription",
                table: "UserSkillMatchReport");

            migrationBuilder.DropColumn(
                name: "JobPostId",
                table: "UserSkillMatchReport");
        }
    }
}
