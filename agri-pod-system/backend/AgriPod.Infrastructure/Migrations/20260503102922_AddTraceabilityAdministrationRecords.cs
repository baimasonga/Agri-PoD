using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriPod.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTraceabilityAdministrationRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BarcodeTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Token = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    EntityReference = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    GeneratedByUserId = table.Column<string>(type: "TEXT", maxLength: 96, nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    IsRevoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarcodeTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceBindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 96, nullable: false),
                    DistrictCode = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    BoundUserEmail = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    BoundAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceBindings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockReconciliations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CampaignId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CampaignName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    LoadedQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    DeliveredQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    ReturnedQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    DiscrepancyQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    SupervisorUserId = table.Column<string>(type: "TEXT", maxLength: 96, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 800, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ReviewStatus = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReconciliations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OtpExpiryMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultGeofenceRadiusMeters = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    DeviceBindingRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    BiometricEvidenceRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    OfflineSyncEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SensitiveDataPolicy = table.Column<string>(type: "TEXT", maxLength: 800, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chiefdoms_DistrictCode_Name",
                table: "Chiefdoms",
                columns: new[] { "DistrictCode", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeTokens_Token",
                table: "BarcodeTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeTokens_TokenType_EntityReference",
                table: "BarcodeTokens",
                columns: new[] { "TokenType", "EntityReference" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceBindings_BoundUserEmail_Status",
                table: "DeviceBindings",
                columns: new[] { "BoundUserEmail", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceBindings_DeviceId",
                table: "DeviceBindings",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockReconciliations_CampaignId_SubmittedAt",
                table: "StockReconciliations",
                columns: new[] { "CampaignId", "SubmittedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarcodeTokens");

            migrationBuilder.DropTable(
                name: "DeviceBindings");

            migrationBuilder.DropTable(
                name: "StockReconciliations");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_Chiefdoms_DistrictCode_Name",
                table: "Chiefdoms");
        }
    }
}
