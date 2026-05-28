using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Costify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipesAndStockCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipes_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockCounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    CountDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockCounts_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockCountItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockCountId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TheoreticalQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CountedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockCountItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockCountItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockCountItems_StockCounts_StockCountId",
                        column: x => x.StockCountId,
                        principalTable: "StockCounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Recipes",
                columns: new[] { "Id", "BusinessId", "Category", "CreatedAt", "Description", "IsActive", "IsDeleted", "Name", "SellingPrice", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, "Sıcak İçecek", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Espresso", 45m, null },
                    { 2, 1, "Sıcak İçecek", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Latte", 65m, null },
                    { 3, 1, "Sıcak İçecek", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Cappuccino", 60m, null },
                    { 4, 1, "Sıcak İçecek", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Türk Çayı", 25m, null },
                    { 5, 1, "Soğuk İçecek", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Buzlu Latte", 75m, null }
                });

            migrationBuilder.InsertData(
                table: "RecipeIngredients",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "ProductId", "Quantity", "RecipeId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, 0.018m, 1, null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, 0.018m, 2, null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 3, 0.18m, 2, null },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 7, 1m, 2, null },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 8, 1m, 2, null },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, 0.018m, 3, null },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 3, 0.12m, 3, null },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 7, 1m, 3, null },
                    { 9, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 8, 1m, 3, null },
                    { 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 10, 0.01m, 4, null },
                    { 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 7, 1m, 4, null },
                    { 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, 0.018m, 5, null },
                    { 13, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 3, 0.2m, 5, null },
                    { 14, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 7, 1m, 5, null },
                    { 15, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 8, 1m, 5, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_ProductId",
                table: "RecipeIngredients",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_RecipeId",
                table: "RecipeIngredients",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_BusinessId",
                table: "Recipes",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_StockCountItems_ProductId",
                table: "StockCountItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockCountItems_StockCountId",
                table: "StockCountItems",
                column: "StockCountId");

            migrationBuilder.CreateIndex(
                name: "IX_StockCounts_BusinessId",
                table: "StockCounts",
                column: "BusinessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeIngredients");

            migrationBuilder.DropTable(
                name: "StockCountItems");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "StockCounts");
        }
    }
}
