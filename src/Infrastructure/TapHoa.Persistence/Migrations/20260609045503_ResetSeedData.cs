using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TapHoa.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ResetSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
TRUNCATE TABLE
    ""UserNotifications"",
    ""PushTokens"",
    ""RefreshTokens"",
    ""WalletTransactions"",
    ""WalletTopupRequests"",
    ""WalletWithdrawRequests"",
    ""CartItems"",
    ""OrderItems"",
    ""Orders"",
    ""Claims"",
    ""Vouchers"",
    ""Addresses"",
    ""Reviews"",
    ""UserHubs"",
    ""HubInventories"",
    ""FlashSaleItems"",
    ""FlashSaleSessions"",
    ""Products"",
    ""Categories"",
    ""Hubs"",
    ""Warehouses"",
    ""Users""
CASCADE;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
