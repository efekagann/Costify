using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Costify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Businesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Businesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorHex = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendors_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    CurrentStock = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MinimumStock = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LastPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransactions_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransactions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransactions_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Businesses",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "IsActive", "IsDeleted", "LogoUrl", "Name", "Phone", "TaxNumber", "UpdatedAt" },
                values: new object[] { 1, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "demo@costify.app", true, false, null, "Demo Kafe", null, null, null });

            migrationBuilder.InsertData(
                table: "Units",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "Name", "Symbol", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Kilogram", "kg", null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Gram", "gr", null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Litre", "lt", null },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Mililitre", "ml", null },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Adet", "ad", null },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Paket", "pk", null },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Koli", "kl", null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "BusinessId", "ColorHex", "CreatedAt", "Description", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, "#3B82F6", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "İçecek Hammaddesi", null },
                    { 2, 1, "#10B981", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Yiyecek Hammaddesi", null },
                    { 3, 1, "#F59E0B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Ambalaj Malzemesi", null },
                    { 4, 1, "#EF4444", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Temizlik Malzemesi", null }
                });

            migrationBuilder.InsertData(
                table: "Vendors",
                columns: new[] { "Id", "Address", "BusinessId", "ContactPerson", "CreatedAt", "CurrentBalance", "Email", "IsActive", "IsDeleted", "Name", "Phone", "TaxNumber", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, 1, "Mehmet Yılmaz", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, "siparis@kadikoyKahve.com.tr", true, false, "Kadıköy Kahve A.Ş.", "0212 555 10 01", "1234567890", null },
                    { 2, null, 1, "Ayşe Kaya", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, "info@ozgungida.com", true, false, "Özgün Gıda Ltd.", "0216 555 20 02", null, null },
                    { 3, null, 1, "Ali Demir", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, "satis@ambalajplus.com", true, false, "Ambalaj Plus", "0212 555 30 03", null, null }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "BusinessId", "CategoryId", "CreatedAt", "CurrentStock", "Description", "IsActive", "IsDeleted", "LastPurchasePrice", "MinimumStock", "Name", "SKU", "UnitId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 28.5m, null, true, false, 485m, 5m, "Arabica Kahve Çekirdeği", "KAH-001", 1, null },
                    { 2, 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3m, null, true, false, 320m, 5m, "Robusta Kahve Çekirdeği", "KAH-002", 1, null },
                    { 3, 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 48m, null, true, false, 42m, 10m, "Tam Yağlı Süt", "SUT-001", 3, null },
                    { 4, 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6m, null, true, false, 72m, 5m, "Krema (Barista)", "KRE-001", 3, null },
                    { 5, 1, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22m, null, true, false, 48m, 5m, "Şeker", "SEK-001", 1, null },
                    { 6, 1, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2m, null, true, false, 58m, 5m, "Un", "UN-001", 1, null },
                    { 7, 1, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 450m, null, true, false, 1.2m, 100m, "Bardak 8oz", "AML-001", 5, null },
                    { 8, 1, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 380m, null, true, false, 0.8m, 100m, "Bardak Kapağı", "AML-002", 5, null },
                    { 9, 1, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25m, null, true, false, 22m, 5m, "Peçete (100'lü Paket)", "AML-003", 6, null },
                    { 10, 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18m, null, true, false, 85m, 10m, "Çay Poşeti", "CAY-001", 6, null },
                    { 11, 1, 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4m, null, true, false, 95m, 2m, "Deterjan (Genel Temizlik)", "TEM-001", 3, null },
                    { 12, 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, null, true, false, 8.5m, 24m, "Soda (330ml)", "SOD-001", 5, null }
                });

            migrationBuilder.InsertData(
                table: "PurchaseOrders",
                columns: new[] { "Id", "BusinessId", "CreatedAt", "DeliveryDate", "IsDeleted", "Notes", "OrderDate", "OrderNumber", "Status", "TotalAmount", "UpdatedAt", "VendorId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), false, "İlk sipariş", new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "SIP-202501-0001", 2, 17320m, null, 1 },
                    { 2, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new DateTime(2025, 1, 22, 0, 0, 0, 0, DateTimeKind.Utc), "SIP-202501-0002", 1, 2680m, null, 2 }
                });

            migrationBuilder.InsertData(
                table: "PurchaseOrderItems",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "ProductId", "PurchaseOrderId", "Quantity", "ReceivedQuantity", "UnitPrice", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, 1, 30m, 30m, 485m, null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 3, 1, 50m, 50m, 42m, null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 5, 2, 20m, 0m, 48m, null },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 6, 2, 10m, 0m, 58m, null },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 9, 2, 20m, 0m, 22m, null }
                });

            migrationBuilder.InsertData(
                table: "StockTransactions",
                columns: new[] { "Id", "BusinessId", "CreatedAt", "IsDeleted", "Notes", "ProductId", "PurchaseOrderId", "Quantity", "TransactionDate", "Type", "UnitCost", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, null, 1, 1, 30m, new DateTime(2025, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), 1, 485m, null },
                    { 2, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, null, 3, 1, 50m, new DateTime(2025, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), 1, 42m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_BusinessId",
                table: "Categories",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BusinessId_SKU",
                table: "Products",
                columns: new[] { "BusinessId", "SKU" },
                unique: true,
                filter: "[SKU] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitId",
                table: "Products",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_ProductId",
                table: "PurchaseOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId",
                table: "PurchaseOrderItems",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_BusinessId_OrderNumber",
                table: "PurchaseOrders",
                columns: new[] { "BusinessId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_VendorId",
                table: "PurchaseOrders",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_BusinessId",
                table: "StockTransactions",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_ProductId",
                table: "StockTransactions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_PurchaseOrderId",
                table: "StockTransactions",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_BusinessId",
                table: "Vendors",
                column: "BusinessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrderItems");

            migrationBuilder.DropTable(
                name: "StockTransactions");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropTable(
                name: "Businesses");
        }
    }
}
