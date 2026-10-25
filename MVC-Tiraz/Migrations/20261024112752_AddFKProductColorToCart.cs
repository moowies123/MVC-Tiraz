using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Tiraz.Migrations
{
    /// <inheritdoc />
    public partial class AddFKProductColorToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductColorId",
                table: "Carts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_ProductColorId",
                table: "Carts",
                column: "ProductColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_ProductColors_ProductColorId",
                table: "Carts",
                column: "ProductColorId",
                principalTable: "ProductColors",
                principalColumn: "ProductColorId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_ProductColors_ProductColorId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_ProductColorId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "ProductColorId",
                table: "Carts");
        }
    }
}
