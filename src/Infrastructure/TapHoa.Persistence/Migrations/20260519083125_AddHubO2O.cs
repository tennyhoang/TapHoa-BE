using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TapHoa.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHubO2O : Migration
    {
        /// <inheritdoc />
        // ID của Hub mặc định dùng để backfill các Order cũ tạo trước khi có O2O.
        private static readonly Guid DefaultHubId = new("11111111-1111-1111-1111-111111111111");

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Tạo bảng Hubs trước để FK từ Orders có thể tham chiếu tới
            migrationBuilder.CreateTable(
                name: "Hubs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hubs", x => x.Id);
                });

            // 2. Seed Hub mặc định để backfill các Order cũ
            migrationBuilder.InsertData(
                table: "Hubs",
                columns: ["Id", "Name", "Address", "Ward", "District", "City",
                          "Latitude", "Longitude", "Status", "CreatedAt"],
                values: [DefaultHubId, "[Hub Mặc Định]", "Chưa xác định", "", "", "",
                         0.0, 0.0, "Inactive", DateTime.UtcNow]);

            // 3. Thêm cột HubId vào Orders (nullable tạm thời để tránh lỗi khi có dữ liệu cũ)
            migrationBuilder.AddColumn<Guid>(
                name: "HubId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            // 4. Backfill tất cả Order cũ → Hub mặc định
            migrationBuilder.Sql(
                $"UPDATE \"Orders\" SET \"HubId\" = '{DefaultHubId}' WHERE \"HubId\" IS NULL");

            // 5. Đổi sang NOT NULL sau khi đã backfill xong
            migrationBuilder.AlterColumn<Guid>(
                name: "HubId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "UserHubs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HubId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHubs_Hubs_HubId",
                        column: x => x.HubId,
                        principalTable: "Hubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserHubs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_HubId",
                table: "Orders",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHubs_HubId",
                table: "UserHubs",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHubs_UserId_HubId",
                table: "UserHubs",
                columns: new[] { "UserId", "HubId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Hubs_HubId",
                table: "Orders",
                column: "HubId",
                principalTable: "Hubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Hubs_HubId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "UserHubs");

            migrationBuilder.DropTable(
                name: "Hubs");

            migrationBuilder.DropIndex(
                name: "IX_Orders_HubId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HubId",
                table: "Orders");
        }
    }
}
