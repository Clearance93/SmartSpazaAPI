using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartData.Migrations
{
    /// <inheritdoc />
    public partial class FixingTheDeliveryRatingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliveryRecordId",
                table: "DeliveryRatings",
                newName: "DeliveryRatingId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Ratings",
                table: "DeliveryRatings",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliveryRatingId",
                table: "DeliveryRatings",
                newName: "DeliveryRecordId");

            migrationBuilder.AlterColumn<string>(
                name: "Ratings",
                table: "DeliveryRatings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }
    }
}
