using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TapHoa.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewOrderStatusLogistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivedAtHubAt",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ShippingAt",
                table: "Orders",
                newName: "ShippingToHubAt");

            migrationBuilder.RenameColumn(
                name: "DeliveredAt",
                table: "Orders",
                newName: "InHubAt");

            migrationBuilder.RenameColumn(
                name: "ConfirmedAt",
                table: "Orders",
                newName: "CompletedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShippingToHubAt",
                table: "Orders",
                newName: "ShippingAt");

            migrationBuilder.RenameColumn(
                name: "InHubAt",
                table: "Orders",
                newName: "DeliveredAt");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "Orders",
                newName: "ConfirmedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivedAtHubAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
