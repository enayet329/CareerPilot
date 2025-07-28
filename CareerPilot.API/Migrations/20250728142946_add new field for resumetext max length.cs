using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerPilot.API.Migrations
{
    /// <inheritdoc />
    public partial class addnewfieldforresumetextmaxlength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResumeText",
                table: "UserFiles",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResumeText",
                table: "UserFiles");
        }
    }
}
