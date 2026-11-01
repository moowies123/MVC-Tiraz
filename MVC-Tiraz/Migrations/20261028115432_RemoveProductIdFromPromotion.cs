using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Tiraz.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductIdFromPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Promotions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Promotions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
