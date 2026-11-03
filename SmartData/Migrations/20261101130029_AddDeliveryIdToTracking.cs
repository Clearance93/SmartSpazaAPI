using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartData.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryIdToTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryId",
                table: "DeliveryTracking",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryId",
                table: "DeliveryTracking");
        }
    }
}
