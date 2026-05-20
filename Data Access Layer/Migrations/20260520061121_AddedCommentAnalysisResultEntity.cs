using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_Access_Layer.Migrations
{
    /// <inheritdoc />
    public partial class AddedCommentAnalysisResultEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommentAnalysisResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToxicityScore = table.Column<double>(type: "float(18)", precision: 18, scale: 4, nullable: false),
                    IsToxic = table.Column<bool>(type: "bit", nullable: false),
                    SentimentScore = table.Column<double>(type: "float(18)", precision: 18, scale: 4, nullable: false, defaultValue: 0.0),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentAnalysisResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentAnalysisResults_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentAnalysisResults_CommentId",
                table: "CommentAnalysisResults",
                column: "CommentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentAnalysisResults_IsToxic",
                table: "CommentAnalysisResults",
                column: "IsToxic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentAnalysisResults");
        }
    }
}
