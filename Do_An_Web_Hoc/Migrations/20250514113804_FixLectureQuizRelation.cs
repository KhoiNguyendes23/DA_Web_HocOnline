using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_An_Web_Hoc.Migrations
{
    /// <inheritdoc />
    public partial class FixLectureQuizRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LectureID",
                table: "Quizzes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Lectures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_LectureID",
                table: "Quizzes",
                column: "LectureID",
                unique: true,
                filter: "[LectureID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Lectures_LectureID",
                table: "Quizzes",
                column: "LectureID",
                principalTable: "Lectures",
                principalColumn: "LectureID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Lectures_LectureID",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_LectureID",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "LectureID",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Lectures");
        }
    }
}
