using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_Access_Layer.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewPropsToOrderEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppliedDiscountId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AppliedDiscountId",
                table: "Orders",
                column: "AppliedDiscountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Discounts_AppliedDiscountId",
                table: "Orders",
                column: "AppliedDiscountId",
                principalTable: "Discounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Discounts_AppliedDiscountId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AppliedDiscountId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AppliedDiscountId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "Orders");
        }
    }
}
