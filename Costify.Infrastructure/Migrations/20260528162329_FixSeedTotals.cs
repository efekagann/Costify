using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Costify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedTotals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PurchaseOrders",
                keyColumn: "Id",
                keyValue: 1,
                column: "TotalAmount",
                value: 16650m);

            migrationBuilder.UpdateData(
                table: "PurchaseOrders",
                keyColumn: "Id",
                keyValue: 2,
                column: "TotalAmount",
                value: 1980m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PurchaseOrders",
                keyColumn: "Id",
                keyValue: 1,
                column: "TotalAmount",
                value: 17320m);

            migrationBuilder.UpdateData(
                table: "PurchaseOrders",
                keyColumn: "Id",
                keyValue: 2,
                column: "TotalAmount",
                value: 2680m);
        }
    }
}
