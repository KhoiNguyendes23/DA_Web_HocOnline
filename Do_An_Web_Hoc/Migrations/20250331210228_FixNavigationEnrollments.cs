using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_An_Web_Hoc.Migrations
{
    /// <inheritdoc />
    public partial class FixNavigationEnrollments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Courses_CoursesCourseID",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_CoursesCourseID",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "CoursesCourseID",
                table: "Enrollments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoursesCourseID",
                table: "Enrollments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CoursesCourseID",
                table: "Enrollments",
                column: "CoursesCourseID");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Courses_CoursesCourseID",
                table: "Enrollments",
                column: "CoursesCourseID",
                principalTable: "Courses",
                principalColumn: "CourseID");
        }
    }
}
