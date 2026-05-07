using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_Access_Layer.Migrations
{
    /// <inheritdoc />
    public partial class mg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerSegmentationResult_Users_AppUserId",
                table: "CustomerSegmentationResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerSegmentationResult",
                table: "CustomerSegmentationResult");

            migrationBuilder.RenameTable(
                name: "CustomerSegmentationResult",
                newName: "CustomerSegmentationResults");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerSegmentationResult_AppUserId",
                table: "CustomerSegmentationResults",
                newName: "IX_CustomerSegmentationResults_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerSegmentationResults",
                table: "CustomerSegmentationResults",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerSegmentationResults_Users_AppUserId",
                table: "CustomerSegmentationResults",
                column: "AppUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerSegmentationResults_Users_AppUserId",
                table: "CustomerSegmentationResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerSegmentationResults",
                table: "CustomerSegmentationResults");

            migrationBuilder.RenameTable(
                name: "CustomerSegmentationResults",
                newName: "CustomerSegmentationResult");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerSegmentationResults_AppUserId",
                table: "CustomerSegmentationResult",
                newName: "IX_CustomerSegmentationResult_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerSegmentationResult",
                table: "CustomerSegmentationResult",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerSegmentationResult_Users_AppUserId",
                table: "CustomerSegmentationResult",
                column: "AppUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
