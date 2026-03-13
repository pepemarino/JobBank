using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBank.Migrations.JobBankIdentityDb
{
    /// <inheritdoc />
    public partial class AdditionalIdentityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CipherText",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LLModel",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nonce",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CipherText",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LLModel",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Nonce",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "AspNetUsers");
        }
    }
}
