using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveItemQuantityColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_Quantity",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Items_Quantity",
                table: "Items",
                column: "Quantity",
                filter: "\"IsDeleted\" = false AND \"Quantity\" > 0");
        }
    }
}
