using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Tiraz.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCartModel2prop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PromotionCode",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PromotionDiscount",
                table: "Carts",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PromotionCode",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "PromotionDiscount",
                table: "Carts");
        }
    }
}
