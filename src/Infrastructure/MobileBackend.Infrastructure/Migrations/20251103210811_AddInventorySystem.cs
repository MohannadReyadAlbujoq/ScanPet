using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventorySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RefundedToInventoryId",
                table: "OrderItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemInventories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MinimumQuantity = table.Column<int>(type: "integer", nullable: true),
                    MaximumQuantity = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemInventories_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemInventories_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_RefundedToInventoryId",
                table: "OrderItems",
                column: "RefundedToInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CreatedBy",
                table: "Inventories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_IsActive",
                table: "Inventories",
                column: "IsActive",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_IsDeleted",
                table: "Inventories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_Name",
                table: "Inventories",
                column: "Name",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventories_CreatedBy",
                table: "ItemInventories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventories_InventoryId",
                table: "ItemInventories",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventories_IsDeleted",
                table: "ItemInventories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventories_ItemId",
                table: "ItemInventories",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventories_ItemId_InventoryId",
                table: "ItemInventories",
                columns: new[] { "ItemId", "InventoryId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Inventories_RefundedToInventoryId",
                table: "OrderItems",
                column: "RefundedToInventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Inventories_RefundedToInventoryId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "ItemInventories");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_RefundedToInventoryId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "RefundedToInventoryId",
                table: "OrderItems");
        }
    }
}
