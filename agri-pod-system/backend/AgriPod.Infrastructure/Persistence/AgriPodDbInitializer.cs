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
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Infrastructure.Persistence;

public static class AgriPodDbInitializer
{
    public static async Task InitializeAsync(AgriPodDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

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

        if (!await db.AppUsers.AnyAsync(cancellationToken))
        {
            db.AppUsers.AddRange(
                new AppUser("Admin User", "admin@agripod.local", "+23276000001", SystemRole.SystemAdministrator, "WESTERN"),
                new AppUser("Project Manager", "manager@agripod.local", "+23276000005", SystemRole.ProjectManager, "WESTERN"),
                new AppUser("Procurement Officer", "procurement@agripod.local", "+23276000006", SystemRole.ProcurementOfficer, "WESTERN"),
                new AppUser("Warehouse Manager", "warehouse@agripod.local", "+23276000007", SystemRole.WarehouseManager, "BOMBALI"),
                new AppUser("District Coordinator", "coordinator@agripod.local", "+23276000002", SystemRole.DistrictCoordinator, "BOMBALI"),
                new AppUser("Field Officer", "field@agripod.local", "+23276000003", SystemRole.FieldOfficer, "BOMBALI"),
                new AppUser("Driver", "driver@agripod.local", "+23276000008", SystemRole.Driver, "BOMBALI"),
                new AppUser("M&E Officer", "me@agripod.local", "+23276000009", SystemRole.MonitoringEvaluationOfficer, "WESTERN"),
                new AppUser("Auditor", "audit@agripod.local", "+23276000004", SystemRole.Auditor, "WESTERN"));
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

        await db.SaveChangesAsync(cancellationToken);
    }
}
