using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_An_Web_Hoc.Migrations
{
    /// <inheritdoc />
    public partial class FixChatForeignKeyRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddForeignKey(
            //    name: "FK_ChatMessages_UserAccounts_SenderId",
            //    table: "ChatMessages",
            //    column: "SenderId",
            //    principalTable: "UserAccounts",
            //    principalColumn: "UserID",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_ChatMessages_UserAccounts_ReceiverId",
            //    table: "ChatMessages",
            //    column: "ReceiverId",
            //    principalTable: "UserAccounts",
            //    principalColumn: "UserID",
            //    onDelete: ReferentialAction.Restrict);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_ChatMessages_UserAccounts_SenderId",
            //    table: "ChatMessages");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_ChatMessages_UserAccounts_ReceiverId",
            //    table: "ChatMessages");
        }
    }
}
