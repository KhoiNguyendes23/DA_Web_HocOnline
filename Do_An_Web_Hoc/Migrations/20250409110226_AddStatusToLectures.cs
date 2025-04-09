using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_An_Web_Hoc.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToLectures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Lectures",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Lectures");
        }
    }
}
