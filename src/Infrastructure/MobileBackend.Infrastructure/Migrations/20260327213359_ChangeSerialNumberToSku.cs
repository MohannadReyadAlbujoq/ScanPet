using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSerialNumberToSku : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderItems_SerialNumber",
                table: "OrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SerialNumber",
                table: "OrderItems",
                column: "SerialNumber",
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderItems_SerialNumber",
                table: "OrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SerialNumber",
                table: "OrderItems",
                column: "SerialNumber",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }
    }
}
