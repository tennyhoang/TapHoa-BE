using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TapHoa.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseManagerRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ManagedWarehouseId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ManagedWarehouseId",
                table: "Users",
                column: "ManagedWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_WarehouseId",
                table: "Users",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Warehouses_ManagedWarehouseId",
                table: "Users",
                column: "ManagedWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Warehouses_WarehouseId",
                table: "Users",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Warehouses_ManagedWarehouseId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Warehouses_WarehouseId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ManagedWarehouseId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_WarehouseId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ManagedWarehouseId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Users");
        }
    }
}
