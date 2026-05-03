using AgriPod.Domain.Administration;
using AgriPod.Domain.Campaigns;
using AgriPod.Domain.Compliance;
using AgriPod.Domain.Deliveries;
using AgriPod.Domain.Dispatch;
using AgriPod.Domain.Farmers;
using AgriPod.Domain.Fleet;
using AgriPod.Domain.Inventory;
using AgriPod.Domain.Procurement;
using AgriPod.Domain.Security;
using AgriPod.Domain.Traceability;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Infrastructure.Persistence;

public static class AgriPodDbInitializer
{
    public static async Task InitializeAsync(AgriPodDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);
        await EnsurePostFeatureTablesAsync(db, cancellationToken);

        if (!await db.Districts.AnyAsync(cancellationToken))
        {
            db.Districts.AddRange(
                new District("WESTERN", "Western Area"),
                new District("BOMBALI", "Bombali"),
                new District("BO", "Bo"),
                new District("KONO", "Kono"),
                new District("PUJEHUN", "Pujehun"),
                new District("KENEMA", "Kenema"));
        }

        if (!await db.Chiefdoms.AnyAsync(cancellationToken))
        {
            db.Chiefdoms.AddRange(
                new Chiefdom("BOMBALI", "Bombali Sebora"),
                new Chiefdom("BO", "Tikonko"),
                new Chiefdom("KONO", "Gbense"),
                new Chiefdom("PUJEHUN", "Kpaka"),
                new Chiefdom("KENEMA", "Nongowa"));
        }

        if (!await db.SystemSettings.AnyAsync(cancellationToken))
        {
            db.SystemSettings.Add(new SystemSetting(
                10,
                150m,
                true,
                true,
                true,
                "Biometric images and OTP proof are stored as secure references and audited on every PoD event."));
        }

        if (!await db.AppUsers.AnyAsync(cancellationToken))
        {
            AddDemoUsers(db);
        }
        else
        {
            AddMissingDemoUsers(db, await db.AppUsers.Select(x => x.Email).ToListAsync(cancellationToken));
        }

        if (!await db.Farmers.AnyAsync(cancellationToken))
        {
            var pending = new Farmer("Aminata Kamara", "SL-NIN-00042", "+23276000010", "BOMBALI", "Bombali Sebora", "Makeni", "Rice", 8.889m, -12.044m, "seed://photos/aminata.jpg");
            var approved = new Farmer("Joseph Conteh", "SL-NIN-00043", "+23276000011", "BO", "Tikonko", "Tikonko", "Cassava", 7.934m, -11.738m, "seed://photos/joseph.jpg");
            approved.Review(true, "Eligible farmer verified by district coordinator.");

            db.Farmers.AddRange(pending, approved);
        }

        if (!await db.InventoryItems.AnyAsync(cancellationToken))
        {
            db.InventoryItems.AddRange(
                new InventoryItem("SEED-RICE-50", "NERICA rice seed", "Seed", "kg"),
                new InventoryItem("FERT-NPK-25", "NPK fertilizer", "Fertilizer", "bag"),
                new InventoryItem("TOOL-SPRAYER", "Manual sprayer", "Equipment", "unit"));
        }

        if (!await db.Warehouses.AnyAsync(cancellationToken))
        {
            db.Warehouses.AddRange(
                new Warehouse("WH-WESTERN", "National stores", "Western Area"),
                new Warehouse("WH-BOMBALI", "Bombali district warehouse", "Bombali"),
                new Warehouse("WH-BO", "Bo district warehouse", "Bo"),
                new Warehouse("WH-KONO", "Kono district warehouse", "Kono"));
        }

        await db.SaveChangesAsync(cancellationToken);

        var rice = await db.InventoryItems.FirstAsync(x => x.Sku == "SEED-RICE-50", cancellationToken);
        var fertilizer = await db.InventoryItems.FirstAsync(x => x.Sku == "FERT-NPK-25", cancellationToken);
        var bombaliWarehouse = await db.Warehouses.FirstAsync(x => x.Code == "WH-BOMBALI", cancellationToken);
        var boWarehouse = await db.Warehouses.FirstAsync(x => x.Code == "WH-BO", cancellationToken);

        if (!await db.StockLots.AnyAsync(cancellationToken))
        {
            db.StockLots.AddRange(
                new StockLot(rice.Id, bombaliWarehouse.Id, "BR-0426-A", 4800m, new DateOnly(2027, 4, 30)),
                new StockLot(fertilizer.Id, bombaliWarehouse.Id, "NP-0426-K", 9120m, new DateOnly(2028, 1, 31)),
                new StockLot(fertilizer.Id, boWarehouse.Id, "NP-0426-B", 6400m, new DateOnly(2028, 1, 31)));
        }

        if (!await db.Suppliers.AnyAsync(cancellationToken))
        {
            var supplier = new Supplier("Sierra Agro Supply", "+23276002000");
            db.Suppliers.Add(supplier);
            db.PurchaseOrders.Add(new PurchaseOrder("PO-2026-018", supplier.Id, fertilizer.Id, 9120m, "NP-0426-K"));
        }

        if (!await db.Campaigns.AnyAsync(cancellationToken))
        {
            var campaign = new Campaign("Wet season seed support", "BOMBALI", "Rice", new DateOnly(2026, 5, 15), new DateOnly(2026, 6, 15), 8.889m, -12.044m, 150m);
            campaign.SubmitForApproval();
            campaign.Approve();
            db.Campaigns.Add(campaign);
        }

        await db.SaveChangesAsync(cancellationToken);

        var wetSeason = await db.Campaigns.FirstAsync(x => x.Name == "Wet season seed support", cancellationToken);
        var approvedFarmers = await db.Farmers.Where(x => x.Status == FarmerStatus.Approved).ToListAsync(cancellationToken);

        if (!await db.FarmerAllocations.AnyAsync(cancellationToken))
        {
            foreach (var farmer in approvedFarmers)
            {
                db.FarmerAllocations.Add(new FarmerAllocation(wetSeason.Id, farmer.Id, rice.Id, 50m));
            }
        }

        if (!await db.Vehicles.AnyAsync(cancellationToken))
        {
            db.Vehicles.AddRange(
                new Vehicle("SL-AG-104", "driver-01", true),
                new Vehicle("SL-AG-118", "driver-02", true),
                new Vehicle("SL-AG-126", "driver-03", true));
        }

        if (!await db.VehicleDispatches.AnyAsync(cancellationToken))
        {
            var dispatch = new VehicleDispatch(wetSeason.Id, bombaliWarehouse.Id, "SL-AG-104", "driver-01");
            dispatch.ConfirmLoaded();
            dispatch.Dispatch();
            db.VehicleDispatches.Add(dispatch);
        }

        if (!await db.VehicleLocations.AnyAsync(cancellationToken))
        {
            db.VehicleLocations.AddRange(
                new VehicleLocation("SL-AG-104", 8.889m, -12.044m, 38m, DateTimeOffset.UtcNow.AddMinutes(-2)),
                new VehicleLocation("SL-AG-118", 8.701m, -11.889m, 42m, DateTimeOffset.UtcNow.AddMinutes(-7)),
                new VehicleLocation("SL-AG-126", 7.934m, -11.738m, 0m, DateTimeOffset.UtcNow.AddMinutes(-1)));
        }

        if (!await db.Deliveries.AnyAsync(cancellationToken))
        {
            var delivery = new Delivery("POD-SL-2026-0018", bombaliWarehouse.Id, "Makeni Farmers Union", "+23276000010", "Bombali");
            delivery.AddLine(rice.Id, 50m, "PKG-RICE-0001");
            db.Deliveries.Add(delivery);
        }

        if (!await db.ExceptionCases.AnyAsync(cancellationToken))
        {
            db.ExceptionCases.Add(new ExceptionCase("GPSMismatch", "High", "Vehicle SL-AG-118 moved outside planned route boundary.", "Vehicle", Guid.Empty));
        }

        if (!await db.AuditLogs.AnyAsync(cancellationToken))
        {
            db.AuditLogs.Add(new AuditLog("system", "DevelopmentSeeded", "System", null, "{}"));
        }

        if (!await db.DeviceBindings.AnyAsync(cancellationToken))
        {
            db.DeviceBindings.Add(new DeviceBinding("FIELD-DEMO-001", "BOMBALI", "field@agripod.local"));
        }

        if (!await db.BarcodeTokens.AnyAsync(cancellationToken))
        {
            db.BarcodeTokens.AddRange(
                new BarcodeToken("FARMER", "FARMER-SEED-0001", "SL-NIN-00043", "system"),
                new BarcodeToken("PACKAGE", "PACKAGE-SEED-0001", "PKG-RICE-0001", "system"),
                new BarcodeToken("MANIFEST", "MANIFEST-SEED-0001", "SL-AG-104", "system"));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void AddDemoUsers(AgriPodDbContext db) =>
        db.AppUsers.AddRange(DemoUsers());

    private static void AddMissingDemoUsers(AgriPodDbContext db, IReadOnlyCollection<string> existingEmails)
    {
        foreach (var user in DemoUsers().Where(user => !existingEmails.Contains(user.Email, StringComparer.OrdinalIgnoreCase)))
        {
            db.AppUsers.Add(user);
        }
    }

    private static AppUser[] DemoUsers() =>
    [
        new AppUser("Admin User", "admin@agripod.local", "+23276000001", SystemRole.SystemAdministrator, "WESTERN"),
        new AppUser("Project Manager", "manager@agripod.local", "+23276000005", SystemRole.ProjectManager, "WESTERN"),
        new AppUser("Procurement Officer", "procurement@agripod.local", "+23276000006", SystemRole.ProcurementOfficer, "WESTERN"),
        new AppUser("Warehouse Manager", "warehouse@agripod.local", "+23276000007", SystemRole.WarehouseManager, "BOMBALI"),
        new AppUser("District Coordinator", "coordinator@agripod.local", "+23276000002", SystemRole.DistrictCoordinator, "BOMBALI"),
        new AppUser("Field Officer", "field@agripod.local", "+23276000003", SystemRole.FieldOfficer, "BOMBALI"),
        new AppUser("Driver", "driver@agripod.local", "+23276000008", SystemRole.Driver, "BOMBALI"),
        new AppUser("M&E Officer", "me@agripod.local", "+23276000009", SystemRole.MonitoringEvaluationOfficer, "WESTERN"),
        new AppUser("Auditor", "audit@agripod.local", "+23276000004", SystemRole.Auditor, "WESTERN")
    ];

    private static async Task EnsurePostFeatureTablesAsync(AgriPodDbContext db, CancellationToken cancellationToken)
    {
        var provider = db.Database.ProviderName ?? "";
        if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            await EnsureSqliteColumnAsync(db, "ProofOfDeliveryEvent", "SignatureReference", "TEXT", cancellationToken);
            await EnsureSqliteColumnAsync(db, "ProofOfDeliveryEvent", "PhotoEvidenceReference", "TEXT", cancellationToken);
            await EnsureSqliteColumnAsync(db, "ProofOfDeliveryEvent", "OfflineTransactionId", "TEXT NOT NULL DEFAULT ''", cancellationToken);
            await EnsureSqliteColumnAsync(db, "ProofOfDeliveryEvent", "DeliveredAt", "TEXT NOT NULL DEFAULT '1970-01-01T00:00:00+00:00'", cancellationToken);

            await db.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "SystemSettings" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_SystemSettings" PRIMARY KEY,
                    "OtpExpiryMinutes" INTEGER NOT NULL,
                    "DefaultGeofenceRadiusMeters" TEXT NOT NULL,
                    "DeviceBindingRequired" INTEGER NOT NULL,
                    "BiometricEvidenceRequired" INTEGER NOT NULL,
                    "OfflineSyncEnabled" INTEGER NOT NULL,
                    "SensitiveDataPolicy" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NOT NULL,
                    "RowVersion" INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS "DeviceBindings" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_DeviceBindings" PRIMARY KEY,
                    "DeviceId" TEXT NOT NULL,
                    "DistrictCode" TEXT NOT NULL,
                    "BoundUserEmail" TEXT NOT NULL,
                    "Status" TEXT NOT NULL,
                    "BoundAt" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NOT NULL,
                    "RowVersion" INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS "BarcodeTokens" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_BarcodeTokens" PRIMARY KEY,
                    "TokenType" TEXT NOT NULL,
                    "Token" TEXT NOT NULL,
                    "EntityReference" TEXT NOT NULL,
                    "GeneratedByUserId" TEXT NOT NULL,
                    "GeneratedAt" TEXT NOT NULL,
                    "IsRevoked" INTEGER NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NOT NULL,
                    "RowVersion" INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS "StockReconciliations" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_StockReconciliations" PRIMARY KEY,
                    "CampaignId" TEXT NOT NULL,
                    "CampaignName" TEXT NOT NULL,
                    "LoadedQuantity" TEXT NOT NULL,
                    "DeliveredQuantity" TEXT NOT NULL,
                    "ReturnedQuantity" TEXT NOT NULL,
                    "DiscrepancyQuantity" TEXT NOT NULL,
                    "Status" TEXT NOT NULL,
                    "SupervisorUserId" TEXT NOT NULL,
                    "Notes" TEXT NOT NULL,
                    "SubmittedAt" TEXT NOT NULL,
                    "ReviewStatus" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NOT NULL,
                    "RowVersion" INTEGER NOT NULL
                );

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_DeviceBindings_DeviceId" ON "DeviceBindings" ("DeviceId");
                CREATE INDEX IF NOT EXISTS "IX_DeviceBindings_BoundUserEmail_Status" ON "DeviceBindings" ("BoundUserEmail", "Status");
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_BarcodeTokens_Token" ON "BarcodeTokens" ("Token");
                CREATE INDEX IF NOT EXISTS "IX_BarcodeTokens_TokenType_EntityReference" ON "BarcodeTokens" ("TokenType", "EntityReference");
                CREATE INDEX IF NOT EXISTS "IX_StockReconciliations_CampaignId_SubmittedAt" ON "StockReconciliations" ("CampaignId", "SubmittedAt");
                """,
                [],
                cancellationToken);
            return;
        }

        if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            await db.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[SystemSettings]', N'U') IS NULL
                CREATE TABLE [SystemSettings] (
                    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_SystemSettings] PRIMARY KEY,
                    [OtpExpiryMinutes] int NOT NULL,
                    [DefaultGeofenceRadiusMeters] decimal(10,2) NOT NULL,
                    [DeviceBindingRequired] bit NOT NULL,
                    [BiometricEvidenceRequired] bit NOT NULL,
                    [OfflineSyncEnabled] bit NOT NULL,
                    [SensitiveDataPolicy] nvarchar(800) NOT NULL,
                    [CreatedAt] datetimeoffset NOT NULL,
                    [UpdatedAt] datetimeoffset NOT NULL,
                    [RowVersion] bigint NOT NULL
                );

                IF OBJECT_ID(N'[DeviceBindings]', N'U') IS NULL
                CREATE TABLE [DeviceBindings] (
                    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DeviceBindings] PRIMARY KEY,
                    [DeviceId] nvarchar(96) NOT NULL,
                    [DistrictCode] nvarchar(24) NOT NULL,
                    [BoundUserEmail] nvarchar(160) NOT NULL,
                    [Status] nvarchar(40) NOT NULL,
                    [BoundAt] datetimeoffset NOT NULL,
                    [CreatedAt] datetimeoffset NOT NULL,
                    [UpdatedAt] datetimeoffset NOT NULL,
                    [RowVersion] bigint NOT NULL
                );

                IF OBJECT_ID(N'[BarcodeTokens]', N'U') IS NULL
                CREATE TABLE [BarcodeTokens] (
                    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_BarcodeTokens] PRIMARY KEY,
                    [TokenType] nvarchar(32) NOT NULL,
                    [Token] nvarchar(128) NOT NULL,
                    [EntityReference] nvarchar(160) NOT NULL,
                    [GeneratedByUserId] nvarchar(96) NOT NULL,
                    [GeneratedAt] datetimeoffset NOT NULL,
                    [IsRevoked] bit NOT NULL,
                    [CreatedAt] datetimeoffset NOT NULL,
                    [UpdatedAt] datetimeoffset NOT NULL,
                    [RowVersion] bigint NOT NULL
                );

                IF OBJECT_ID(N'[StockReconciliations]', N'U') IS NULL
                CREATE TABLE [StockReconciliations] (
                    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_StockReconciliations] PRIMARY KEY,
                    [CampaignId] uniqueidentifier NOT NULL,
                    [CampaignName] nvarchar(160) NOT NULL,
                    [LoadedQuantity] decimal(18,3) NOT NULL,
                    [DeliveredQuantity] decimal(18,3) NOT NULL,
                    [ReturnedQuantity] decimal(18,3) NOT NULL,
                    [DiscrepancyQuantity] decimal(18,3) NOT NULL,
                    [Status] nvarchar(40) NOT NULL,
                    [SupervisorUserId] nvarchar(96) NOT NULL,
                    [Notes] nvarchar(800) NOT NULL,
                    [SubmittedAt] datetimeoffset NOT NULL,
                    [ReviewStatus] nvarchar(40) NOT NULL,
                    [CreatedAt] datetimeoffset NOT NULL,
                    [UpdatedAt] datetimeoffset NOT NULL,
                    [RowVersion] bigint NOT NULL
                );
                """,
                [],
                cancellationToken);

            await db.Database.ExecuteSqlRawAsync(
                """
                IF COL_LENGTH(N'ProofOfDeliveryEvent', N'SignatureReference') IS NULL
                    ALTER TABLE [ProofOfDeliveryEvent] ADD [SignatureReference] nvarchar(max) NULL;
                IF COL_LENGTH(N'ProofOfDeliveryEvent', N'PhotoEvidenceReference') IS NULL
                    ALTER TABLE [ProofOfDeliveryEvent] ADD [PhotoEvidenceReference] nvarchar(max) NULL;
                IF COL_LENGTH(N'ProofOfDeliveryEvent', N'OfflineTransactionId') IS NULL
                    ALTER TABLE [ProofOfDeliveryEvent] ADD [OfflineTransactionId] nvarchar(max) NOT NULL DEFAULT N'';
                IF COL_LENGTH(N'ProofOfDeliveryEvent', N'DeliveredAt') IS NULL
                    ALTER TABLE [ProofOfDeliveryEvent] ADD [DeliveredAt] datetimeoffset NOT NULL DEFAULT '1970-01-01T00:00:00+00:00';
                """,
                [],
                cancellationToken);
        }
    }

    private static async Task EnsureSqliteColumnAsync(
        AgriPodDbContext db,
        string table,
        string column,
        string definition,
        CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{table}')";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (reader.GetString(1).Equals(column, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        await using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {definition}";
        await alter.ExecuteNonQueryAsync(cancellationToken);
    }
}
