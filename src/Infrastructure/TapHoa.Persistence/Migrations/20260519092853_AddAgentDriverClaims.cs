using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TapHoa.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentDriverClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AgentHubId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderClaims_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderClaims_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_AgentHubId",
                table: "Users",
                column: "AgentHubId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderClaims_CustomerId",
                table: "OrderClaims",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderClaims_OrderId",
                table: "OrderClaims",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Hubs_AgentHubId",
                table: "Users",
                column: "AgentHubId",
                principalTable: "Hubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Hubs_AgentHubId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "OrderClaims");

            migrationBuilder.DropIndex(
                name: "IX_Users_AgentHubId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AgentHubId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefundedAt",
                table: "Orders");
        }
    }
}
