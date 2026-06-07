using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBank.Migrations.JobBankIdentityDb
{
    /// <inheritdoc />
    public partial class IdentityCreateUpdateUserDtMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UserCreated",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UserUpdated",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserCreated",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserUpdated",
                table: "AspNetUsers");
        }
    }
}
