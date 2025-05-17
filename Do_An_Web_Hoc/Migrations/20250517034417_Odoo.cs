using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_An_Web_Hoc.Migrations
{
    /// <inheritdoc />
    public partial class Odoo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_UserAccounts_ReceiverId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_UserAccounts_SenderId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_LectureProgresses_Lectures_LectureID",
                table: "LectureProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_LectureProgresses_UserAccounts_UserId",
                table: "LectureProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_LiveMeetings_UserAccounts_CreatedBy",
                table: "LiveMeetings");

            migrationBuilder.DropIndex(
                name: "IX_LectureProgresses_LectureID",
                table: "LectureProgresses");

            migrationBuilder.DropIndex(
                name: "IX_LectureProgresses_UserId",
                table: "LectureProgresses");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_UserAccounts_ReceiverId",
                table: "ChatMessages",
                column: "ReceiverId",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_UserAccounts_SenderId",
                table: "ChatMessages",
                column: "SenderId",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LiveMeetings_UserAccounts_CreatedBy",
                table: "LiveMeetings",
                column: "CreatedBy",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_UserAccounts_ReceiverId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_UserAccounts_SenderId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_LiveMeetings_UserAccounts_CreatedBy",
                table: "LiveMeetings");

            migrationBuilder.CreateIndex(
                name: "IX_LectureProgresses_LectureID",
                table: "LectureProgresses",
                column: "LectureID");

            migrationBuilder.CreateIndex(
                name: "IX_LectureProgresses_UserId",
                table: "LectureProgresses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_UserAccounts_ReceiverId",
                table: "ChatMessages",
                column: "ReceiverId",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_UserAccounts_SenderId",
                table: "ChatMessages",
                column: "SenderId",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LectureProgresses_Lectures_LectureID",
                table: "LectureProgresses",
                column: "LectureID",
                principalTable: "Lectures",
                principalColumn: "LectureID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LectureProgresses_UserAccounts_UserId",
                table: "LectureProgresses",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LiveMeetings_UserAccounts_CreatedBy",
                table: "LiveMeetings",
                column: "CreatedBy",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
