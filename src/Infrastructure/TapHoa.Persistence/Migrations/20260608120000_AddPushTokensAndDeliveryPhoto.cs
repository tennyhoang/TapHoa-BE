using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TapHoa.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPushTokensAndDeliveryPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""Orders"" ADD COLUMN IF NOT EXISTS ""DeliveryPhotoUrl"" character varying(2048);

CREATE TABLE IF NOT EXISTS ""PushTokens"" (
    ""Id""        uuid                        NOT NULL,
    ""UserId""    uuid                        NOT NULL,
    ""Token""     character varying(200)      NOT NULL,
    ""Platform""  character varying(10),
    ""CreatedAt"" timestamp with time zone    NOT NULL,
    ""UpdatedAt"" timestamp with time zone,
    CONSTRAINT ""PK_PushTokens"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_PushTokens_Users_UserId"" FOREIGN KEY (""UserId"")
        REFERENCES ""Users"" (""Id"") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PushTokens_Token"" ON ""PushTokens"" (""Token"");
CREATE INDEX        IF NOT EXISTS ""IX_PushTokens_UserId"" ON ""PushTokens"" (""UserId"");
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS ""PushTokens"";
ALTER TABLE ""Orders"" DROP COLUMN IF EXISTS ""DeliveryPhotoUrl"";
");
        }
    }
}
