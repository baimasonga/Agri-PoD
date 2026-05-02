using AgriPod.Application.Abstractions;
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

public sealed class AgriPodDbContext(DbContextOptions<AgriPodDbContext> options) : DbContext(options), IAgriPodDbContext
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockLot> StockLots => Set<StockLot>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<VehicleLocation> VehicleLocations => Set<VehicleLocation>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Chiefdom> Chiefdoms => Set<Chiefdom>();
    public DbSet<Farmer> Farmers => Set<Farmer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<FarmerAllocation> FarmerAllocations => Set<FarmerAllocation>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleDispatch> VehicleDispatches => Set<VehicleDispatch>();
    public DbSet<DistributionSession> DistributionSessions => Set<DistributionSession>();
    public DbSet<ExceptionCase> ExceptionCases => Set<ExceptionCase>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.Property(x => x.Sku).HasMaxLength(48);
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.Category).HasMaxLength(80);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32);
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(24);
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.District).HasMaxLength(80);
        });

        modelBuilder.Entity<StockLot>(entity =>
        {
            entity.HasIndex(x => new { x.WarehouseId, x.LotCode }).IsUnique();
            entity.Property(x => x.LotCode).HasMaxLength(64);
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasIndex(x => x.DeliveryNumber).IsUnique();
            entity.Property(x => x.DeliveryNumber).HasMaxLength(48);
            entity.Property(x => x.RecipientName).HasMaxLength(160);
            entity.Property(x => x.RecipientPhone).HasMaxLength(32);
            entity.Property(x => x.DestinationDistrict).HasMaxLength(80);
            entity.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.DeliveryId);
            entity.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasMany(x => x.ProofEvents).WithOne().HasForeignKey(x => x.DeliveryId);
            entity.Navigation(x => x.ProofEvents).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<DeliveryLine>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.Barcode).HasMaxLength(128);
        });

        modelBuilder.Entity<ProofOfDeliveryEvent>(entity =>
        {
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
            entity.Property(x => x.CapturedByUserId).HasMaxLength(96);
            entity.Property(x => x.BiometricReference).HasMaxLength(256);
        });

        modelBuilder.Entity<VehicleLocation>(entity =>
        {
            entity.HasIndex(x => new { x.VehicleRegistration, x.RecordedAt });
            entity.Property(x => x.VehicleRegistration).HasMaxLength(32);
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
            entity.Property(x => x.SpeedKph).HasPrecision(6, 2);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.FullName).HasMaxLength(160);
            entity.Property(x => x.Email).HasMaxLength(160);
            entity.Property(x => x.Phone).HasMaxLength(32);
            entity.Property(x => x.DistrictCode).HasMaxLength(24);
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(24);
            entity.Property(x => x.Name).HasMaxLength(120);
        });

        modelBuilder.Entity<Chiefdom>(entity =>
        {
            entity.Property(x => x.DistrictCode).HasMaxLength(24);
            entity.Property(x => x.Name).HasMaxLength(120);
        });

        modelBuilder.Entity<Farmer>(entity =>
        {
            entity.HasIndex(x => x.BarcodeToken).IsUnique();
            entity.HasIndex(x => new { x.NationalId, x.DistrictCode });
            entity.Property(x => x.FullName).HasMaxLength(160);
            entity.Property(x => x.NationalId).HasMaxLength(80);
            entity.Property(x => x.Phone).HasMaxLength(32);
            entity.Property(x => x.DistrictCode).HasMaxLength(24);
            entity.Property(x => x.Chiefdom).HasMaxLength(120);
            entity.Property(x => x.Community).HasMaxLength(120);
            entity.Property(x => x.ValueChain).HasMaxLength(80);
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
            entity.Property(x => x.PhotoReference).HasMaxLength(256);
            entity.Property(x => x.BarcodeToken).HasMaxLength(96);
            entity.Property(x => x.ReviewComment).HasMaxLength(512);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.ContactPhone).HasMaxLength(32);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasIndex(x => x.PurchaseNumber).IsUnique();
            entity.Property(x => x.PurchaseNumber).HasMaxLength(64);
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.BatchNumber).HasMaxLength(64);
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.DistrictCode).HasMaxLength(24);
            entity.Property(x => x.ValueChain).HasMaxLength(80);
            entity.Property(x => x.SiteLatitude).HasPrecision(9, 6);
            entity.Property(x => x.SiteLongitude).HasPrecision(9, 6);
            entity.Property(x => x.GpsRadiusMeters).HasPrecision(10, 2);
        });

        modelBuilder.Entity<FarmerAllocation>(entity =>
        {
            entity.HasIndex(x => new { x.CampaignId, x.FarmerId, x.InventoryItemId }).IsUnique();
            entity.HasIndex(x => x.AllocationToken).IsUnique();
            entity.Property(x => x.AllocatedQuantity).HasPrecision(18, 3);
            entity.Property(x => x.DeliveredQuantity).HasPrecision(18, 3);
            entity.Property(x => x.AllocationToken).HasMaxLength(96);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(x => x.Registration).IsUnique();
            entity.Property(x => x.Registration).HasMaxLength(32);
            entity.Property(x => x.DriverUserId).HasMaxLength(96);
        });

        modelBuilder.Entity<VehicleDispatch>(entity =>
        {
            entity.HasIndex(x => x.ManifestBarcode).IsUnique();
            entity.Property(x => x.VehicleRegistration).HasMaxLength(32);
            entity.Property(x => x.DriverUserId).HasMaxLength(96);
            entity.Property(x => x.ManifestBarcode).HasMaxLength(96);
        });

        modelBuilder.Entity<DistributionSession>(entity =>
        {
            entity.Property(x => x.FieldOfficerUserId).HasMaxLength(96);
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
        });

        modelBuilder.Entity<ExceptionCase>(entity =>
        {
            entity.Property(x => x.CaseType).HasMaxLength(80);
            entity.Property(x => x.Severity).HasMaxLength(32);
            entity.Property(x => x.Description).HasMaxLength(800);
            entity.Property(x => x.RelatedEntityType).HasMaxLength(80);
            entity.Property(x => x.Status).HasMaxLength(40);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.ActorUserId).HasMaxLength(96);
            entity.Property(x => x.Action).HasMaxLength(120);
            entity.Property(x => x.EntityType).HasMaxLength(80);
        });
    }
}
