using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_Access_Layer.Migrations
{
    /// <inheritdoc />
    public partial class AddedSubCategoryDemandForecastEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubCategoryDemandForecasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetMonth = table.Column<int>(type: "int", nullable: false),
                    TargetYear = table.Column<int>(type: "int", nullable: false),
                    PredictedSalesCount = table.Column<int>(type: "int", nullable: false),
                    PredictedRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ModelAccuracyScore = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategoryDemandForecasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategoryDemandForecasts_SubCategories_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalTable: "SubCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubCatForecast_SubCat_Date",
                table: "SubCategoryDemandForecasts",
                columns: new[] { "SubCategoryId", "TargetYear", "TargetMonth" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubCategoryDemandForecasts");
        }
    }
}
