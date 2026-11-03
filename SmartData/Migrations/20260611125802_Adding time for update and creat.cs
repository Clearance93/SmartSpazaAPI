using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartData.Migrations
{
    /// <inheritdoc />
    public partial class Addingtimeforupdateandcreat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "DeliveryRecord",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "DeliveryRecord",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "DeliveryRecord");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "DeliveryRecord");
        }
    }
}
