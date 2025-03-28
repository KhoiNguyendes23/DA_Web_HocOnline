using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_An_Web_Hoc.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserIDIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ XÓA TOÀN BỘ KHÓA NGOẠI THAM CHIẾU UserAccounts
            migrationBuilder.DropForeignKey(name: "FK_Results_UserAccounts_UserID", table: "Results");
            migrationBuilder.DropForeignKey(name: "FK_UserAnswers_UserAccounts_UserID", table: "UserAnswers");
            migrationBuilder.DropForeignKey(name: "FK_Payments_UserAccounts_UserID", table: "Payments");
            migrationBuilder.DropForeignKey(name: "FK_UserActivities_UserAccounts_UserId", table: "UserActivities");
            migrationBuilder.DropForeignKey(name: "FK_Enrollments_UserAccounts_UserID", table: "Enrollments");
            migrationBuilder.DropForeignKey(name: "FK_Ratings_UserAccounts_UserID", table: "Ratings");
            migrationBuilder.DropForeignKey(name: "FK_Reviews_UserAccounts_UserID", table: "Reviews");
            migrationBuilder.DropForeignKey(name: "FK_Documents_UserAccounts_UploadedBy", table: "Documents");

            // 2️⃣ XÓA KHÓA CHÍNH & CỘT UserID
            migrationBuilder.DropPrimaryKey(name: "PK_UserAccounts", table: "UserAccounts");
            migrationBuilder.DropColumn(name: "UserID", table: "UserAccounts");

            // 3️⃣ THÊM LẠI CỘT UserID CÓ IDENTITY
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "UserAccounts",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // 4️⃣ THÊM LẠI KHÓA CHÍNH
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAccounts",
                table: "UserAccounts",
                column: "UserID");

            // 5️⃣ THÊM LẠI TOÀN BỘ CÁC FOREIGN KEY
            migrationBuilder.AddForeignKey(
                name: "FK_Results_UserAccounts_UserID",
                table: "Results",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_UserAccounts_UserID",
                table: "UserAnswers",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_UserAccounts_UserID",
                table: "Payments",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivities_UserAccounts_UserId",
                table: "UserActivities",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_UserAccounts_UserID",
                table: "Enrollments",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_UserAccounts_UserID",
                table: "Ratings",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_UserAccounts_UserID",
                table: "Reviews",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_UserAccounts_UploadedBy",
                table: "Documents",
                column: "UploadedBy",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }




        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ XÓA TOÀN BỘ CÁC FOREIGN KEY đã thêm trong Up()
            migrationBuilder.DropForeignKey(name: "FK_Results_UserAccounts_UserID", table: "Results");
            migrationBuilder.DropForeignKey(name: "FK_UserAnswers_UserAccounts_UserID", table: "UserAnswers");
            migrationBuilder.DropForeignKey(name: "FK_Payments_UserAccounts_UserID", table: "Payments");
            migrationBuilder.DropForeignKey(name: "FK_UserActivities_UserAccounts_UserId", table: "UserActivities");
            migrationBuilder.DropForeignKey(name: "FK_Enrollments_UserAccounts_UserID", table: "Enrollments");
            migrationBuilder.DropForeignKey(name: "FK_Ratings_UserAccounts_UserID", table: "Ratings");
            migrationBuilder.DropForeignKey(name: "FK_Reviews_UserAccounts_UserID", table: "Reviews");
            migrationBuilder.DropForeignKey(name: "FK_Documents_UserAccounts_UploadedBy", table: "Documents");

            // 2️⃣ XÓA KHÓA CHÍNH mới
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAccounts",
                table: "UserAccounts");

            // 3️⃣ XÓA CỘT UserID có IDENTITY
            migrationBuilder.DropColumn(
                name: "UserID",
                table: "UserAccounts");

            // 4️⃣ THÊM LẠI CỘT UserID không có IDENTITY
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "UserAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 5️⃣ THÊM LẠI KHÓA CHÍNH
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAccounts",
                table: "UserAccounts",
                column: "UserID");

            // 6️⃣ THÊM LẠI TOÀN BỘ FOREIGN KEY đã xóa
            migrationBuilder.AddForeignKey(
                name: "FK_Results_UserAccounts_UserID",
                table: "Results",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_UserAccounts_UserID",
                table: "UserAnswers",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_UserAccounts_UserID",
                table: "Payments",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivities_UserAccounts_UserId",
                table: "UserActivities",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_UserAccounts_UserID",
                table: "Enrollments",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_UserAccounts_UserID",
                table: "Ratings",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_UserAccounts_UserID",
                table: "Reviews",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_UserAccounts_UploadedBy",
                table: "Documents",
                column: "UploadedBy",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }



    }
}
