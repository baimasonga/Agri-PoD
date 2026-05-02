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

namespace AgriPod.Application.Abstractions;

public interface IAgriPodDbContext
{
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<StockLot> StockLots { get; }
    DbSet<Delivery> Deliveries { get; }
    DbSet<VehicleLocation> VehicleLocations { get; }
    DbSet<AppUser> AppUsers { get; }
    DbSet<District> Districts { get; }
    DbSet<Chiefdom> Chiefdoms { get; }
    DbSet<Farmer> Farmers { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<Campaign> Campaigns { get; }
    DbSet<FarmerAllocation> FarmerAllocations { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<VehicleDispatch> VehicleDispatches { get; }
    DbSet<DistributionSession> DistributionSessions { get; }
    DbSet<ExceptionCase> ExceptionCases { get; }
    DbSet<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
