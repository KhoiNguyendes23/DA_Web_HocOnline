using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_An_Web_Hoc.Migrations
{
    /// <inheritdoc />
    public partial class FixChatMessageNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_UserAccounts_UserAccountUserID",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_UserAccounts_UserAccountUserID1",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_UserAccountUserID",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_UserAccountUserID1",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "UserAccountUserID",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "UserAccountUserID1",
                table: "ChatMessages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserAccountUserID",
                table: "ChatMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserAccountUserID1",
                table: "ChatMessages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_UserAccountUserID",
                table: "ChatMessages",
                column: "UserAccountUserID");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_UserAccountUserID1",
                table: "ChatMessages",
                column: "UserAccountUserID1");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_UserAccounts_UserAccountUserID",
                table: "ChatMessages",
                column: "UserAccountUserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_UserAccounts_UserAccountUserID1",
                table: "ChatMessages",
                column: "UserAccountUserID1",
                principalTable: "UserAccounts",
                principalColumn: "UserID");
        }
    }
}
