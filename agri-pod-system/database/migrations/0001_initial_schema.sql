create table dbo.InventoryItems (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    Sku nvarchar(48) not null unique,
    Name nvarchar(160) not null,
    Category nvarchar(80) not null,
    UnitOfMeasure nvarchar(32) not null
);

create table dbo.Warehouses (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    Code nvarchar(24) not null unique,
    Name nvarchar(160) not null,
    District nvarchar(80) not null
);

create table dbo.Deliveries (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    DeliveryNumber nvarchar(48) not null unique,
    OriginWarehouseId uniqueidentifier not null,
    RecipientName nvarchar(160) not null,
    RecipientPhone nvarchar(32) not null,
    DestinationDistrict nvarchar(80) not null,
    Status int not null
);

create table dbo.VehicleLocations (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    VehicleRegistration nvarchar(32) not null,
    Latitude decimal(9, 6) not null,
    Longitude decimal(9, 6) not null,
    SpeedKph decimal(6, 2) null,
    RecordedAt datetimeoffset not null
);

create table dbo.AppUsers (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    FullName nvarchar(160) not null,
    Email nvarchar(160) not null unique,
    Phone nvarchar(32) not null,
    Role int not null,
    DistrictCode nvarchar(24) not null,
    IsActive bit not null
);

create table dbo.Farmers (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    FullName nvarchar(160) not null,
    NationalId nvarchar(80) not null,
    Phone nvarchar(32) not null,
    DistrictCode nvarchar(24) not null,
    Chiefdom nvarchar(120) not null,
    Community nvarchar(120) not null,
    ValueChain nvarchar(80) not null,
    Latitude decimal(9, 6) not null,
    Longitude decimal(9, 6) not null,
    PhotoReference nvarchar(256) not null,
    BarcodeToken nvarchar(96) not null unique,
    Status int not null,
    ReviewComment nvarchar(512) null
);

create table dbo.Campaigns (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    Name nvarchar(160) not null,
    DistrictCode nvarchar(24) not null,
    ValueChain nvarchar(80) not null,
    StartsOn date not null,
    EndsOn date not null,
    SiteLatitude decimal(9, 6) not null,
    SiteLongitude decimal(9, 6) not null,
    GpsRadiusMeters decimal(10, 2) not null,
    Status int not null
);

create table dbo.FarmerAllocations (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    CampaignId uniqueidentifier not null,
    FarmerId uniqueidentifier not null,
    InventoryItemId uniqueidentifier not null,
    AllocatedQuantity decimal(18, 3) not null,
    DeliveredQuantity decimal(18, 3) not null,
    AllocationToken nvarchar(96) not null unique
);

create table dbo.Suppliers (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    Name nvarchar(160) not null,
    ContactPhone nvarchar(32) not null
);

create table dbo.PurchaseOrders (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    PurchaseNumber nvarchar(64) not null unique,
    SupplierId uniqueidentifier not null,
    InventoryItemId uniqueidentifier not null,
    Quantity decimal(18, 3) not null,
    BatchNumber nvarchar(64) not null,
    SubmittedForReceipt bit not null
);

create table dbo.Vehicles (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    Registration nvarchar(32) not null unique,
    DriverUserId nvarchar(96) not null,
    TrackingActive bit not null
);

create table dbo.VehicleDispatches (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    CampaignId uniqueidentifier not null,
    WarehouseId uniqueidentifier not null,
    VehicleRegistration nvarchar(32) not null,
    DriverUserId nvarchar(96) not null,
    ManifestBarcode nvarchar(96) not null unique,
    Status int not null
);

create table dbo.DistributionSessions (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    CampaignId uniqueidentifier not null,
    DispatchId uniqueidentifier not null,
    FieldOfficerUserId nvarchar(96) not null,
    Latitude decimal(9, 6) not null,
    Longitude decimal(9, 6) not null,
    StartedAt datetimeoffset not null,
    ClosedAt datetimeoffset null
);

create table dbo.ExceptionCases (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    CaseType nvarchar(80) not null,
    Severity nvarchar(32) not null,
    Description nvarchar(800) not null,
    RelatedEntityType nvarchar(80) not null,
    RelatedEntityId uniqueidentifier not null,
    Status nvarchar(40) not null
);

create table dbo.AuditLogs (
    Id uniqueidentifier not null primary key,
    CreatedAt datetimeoffset not null,
    UpdatedAt datetimeoffset not null,
    RowVersion bigint not null,
    ActorUserId nvarchar(96) not null,
    Action nvarchar(120) not null,
    EntityType nvarchar(80) not null,
    EntityId uniqueidentifier null,
    MetadataJson nvarchar(max) not null
);
