using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserDefaultsAndOrderInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Locations_LocationId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Inventories_DefaultInventoryId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DefaultInventoryId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DefaultInventoryId",
                table: "Users");

            // Step 1: Add InventoryId as nullable first
            migrationBuilder.AddColumn<Guid>(
                name: "InventoryId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            // Step 2: Migrate existing orders — map LocationId to first inventory at that location
            migrationBuilder.Sql(@"
                UPDATE ""Orders"" o
                SET ""InventoryId"" = (
                    SELECT i.""Id""
                    FROM ""Inventories"" i
                    WHERE i.""LocationId"" = o.""LocationId""
                      AND i.""IsDeleted"" = false
                    LIMIT 1
                )
                WHERE o.""InventoryId"" IS NULL
                  AND o.""LocationId"" IS NOT NULL;
            ");

            // Step 3: For any orders still without an InventoryId, assign the first active inventory
            migrationBuilder.Sql(@"
                UPDATE ""Orders""
                SET ""InventoryId"" = (
                    SELECT ""Id"" FROM ""Inventories""
                    WHERE ""IsDeleted"" = false
                    LIMIT 1
                )
                WHERE ""InventoryId"" IS NULL;
            ");

            // Step 4: Make InventoryId non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "InventoryId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // Step 5: Drop LocationId column and its index
            migrationBuilder.DropIndex(
                name: "IX_Orders_LocationId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Orders");

            // Step 6: Create index on InventoryId
            migrationBuilder.CreateIndex(
                name: "IX_Orders_InventoryId",
                table: "Orders",
                column: "InventoryId");

            migrationBuilder.CreateTable(
                name: "UserDefaultInventories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDefaultInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDefaultInventories_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDefaultInventories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDefaultLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDefaultLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDefaultLocations_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDefaultLocations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDefaultInventories_InventoryId",
                table: "UserDefaultInventories",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDefaultInventories_UserId_InventoryId",
                table: "UserDefaultInventories",
                columns: new[] { "UserId", "InventoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDefaultLocations_LocationId",
                table: "UserDefaultLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDefaultLocations_UserId_LocationId",
                table: "UserDefaultLocations",
                columns: new[] { "UserId", "LocationId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Inventories_InventoryId",
                table: "Orders",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Inventories_InventoryId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "UserDefaultInventories");

            migrationBuilder.DropTable(
                name: "UserDefaultLocations");

            migrationBuilder.RenameColumn(
                name: "InventoryId",
                table: "Orders",
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_InventoryId",
                table: "Orders",
                newName: "IX_Orders_LocationId");

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
                name: "FK_Orders_Locations_LocationId",
                table: "Orders",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Inventories_DefaultInventoryId",
                table: "Users",
                column: "DefaultInventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
