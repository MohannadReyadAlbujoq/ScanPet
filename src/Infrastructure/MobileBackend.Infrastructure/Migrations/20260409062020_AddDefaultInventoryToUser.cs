using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultInventoryToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefaultInventoryId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DefaultInventoryId",
                table: "Users",
                column: "DefaultInventoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Inventories_DefaultInventoryId",
                table: "Users",
                column: "DefaultInventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Inventories_DefaultInventoryId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DefaultInventoryId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DefaultInventoryId",
                table: "Users");
        }
    }
}
