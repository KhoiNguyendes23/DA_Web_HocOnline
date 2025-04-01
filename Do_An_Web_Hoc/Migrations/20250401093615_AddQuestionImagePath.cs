using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_An_Web_Hoc.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Questions");
        }
    }
}
