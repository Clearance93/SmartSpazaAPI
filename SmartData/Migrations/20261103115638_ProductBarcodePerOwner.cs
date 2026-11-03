using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartData.Migrations
{
    /// <inheritdoc />
    public partial class ProductBarcodePerOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode_ShopOwnerId",
                table: "Products",
                columns: new[] { "Barcode", "ShopOwnerId" },
                unique: true,
                filter: "[Barcode] IS NOT NULL AND [ShopOwnerId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode_SupplierId",
                table: "Products",
                columns: new[] { "Barcode", "SupplierId" },
                unique: true,
                filter: "[Barcode] IS NOT NULL AND [SupplierId] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode_ShopOwnerId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode_SupplierId",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL AND [IsDeleted] = 0");
        }
    }
}
