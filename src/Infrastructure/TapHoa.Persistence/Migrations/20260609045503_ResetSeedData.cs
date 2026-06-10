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
DO $$
BEGIN
  TRUNCATE TABLE
    ""UserNotifications"",
    ""PushTokens"",
    ""RefreshTokens"",
    ""WalletTransactions"",
    ""WalletTopupRequests"",
    ""WithdrawRequests"",
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
EXCEPTION WHEN others THEN
  NULL;
END $$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
