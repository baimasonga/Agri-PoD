namespace AgriPod.Shared;

public sealed record InventoryItemDto(Guid Id, string Sku, string Name, string Category, string UnitOfMeasure);

public sealed record StockLotDto(Guid Id, Guid ItemId, string LotCode, string WarehouseCode, decimal Quantity, DateOnly? ExpiresOn);

public sealed record CreateInventoryItemRequest(string Sku, string Name, string Category, string UnitOfMeasure);

public sealed record WarehouseDto(Guid Id, string Code, string Name, string District);

public sealed record CreateWarehouseRequest(string Code, string Name, string District);

public sealed record CreateDeliveryRequest(
    string DeliveryNumber,
    Guid OriginWarehouseId,
    string RecipientName,
    string RecipientPhone,
    string DestinationDistrict,
    IReadOnlyCollection<CreateDeliveryLineRequest> Lines);

public sealed record CreateDeliveryLineRequest(Guid InventoryItemId, decimal Quantity, string? Barcode);

public sealed record DeliveryDto(
    Guid Id,
    string DeliveryNumber,
    string RecipientName,
    string RecipientPhone,
    string DestinationDistrict,
    string Status,
    IReadOnlyCollection<DeliveryLineDto> Lines);

public sealed record DeliveryLineDto(Guid InventoryItemId, decimal Quantity, string? Barcode);

public sealed record CompleteDeliveryRequest(decimal Latitude, decimal Longitude, string CapturedByUserId, string? BiometricReference);

public sealed record SendOtpRequest(string RecipientPhone, string Channel = "sms");

public sealed record VerifyOtpRequest(string RecipientPhone, string Code);

public sealed record OtpResult(bool Succeeded, string Status);

public sealed record VehicleLocationRequest(string VehicleRegistration, decimal Latitude, decimal Longitude, decimal? SpeedKph, DateTimeOffset RecordedAt);

public sealed record SyncRequest(string DeviceId, string DistrictCode, long LastServerVersion, IReadOnlyCollection<SyncMutationDto> Mutations);

public sealed record SyncMutationDto(Guid ClientMutationId, string EntityName, string Operation, string JsonPayload, DateTimeOffset OccurredAt);

public sealed record SyncResponse(long ServerVersion, IReadOnlyCollection<object> Changes, IReadOnlyCollection<Guid> AcceptedMutations);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthTokenResponse(string AccessToken, DateTimeOffset ExpiresAt, string Role);

public sealed record UserDto(Guid Id, string FullName, string Email, string Phone, string Role, string DistrictCode, bool IsActive);

public sealed record CreateUserRequest(string FullName, string Email, string Phone, string Role, string DistrictCode);

public sealed record DistrictDto(Guid Id, string Code, string Name);

public sealed record ChiefdomDto(Guid Id, string DistrictCode, string Name);

public sealed record FarmerDto(
    Guid Id,
    string FullName,
    string NationalId,
    string Phone,
    string DistrictCode,
    string Chiefdom,
    string Community,
    string ValueChain,
    decimal Latitude,
    decimal Longitude,
    string PhotoReference,
    string BarcodeToken,
    string Status);

public sealed record RegisterFarmerRequest(
    string FullName,
    string NationalId,
    string Phone,
    string DistrictCode,
    string Chiefdom,
    string Community,
    string ValueChain,
    decimal Latitude,
    decimal Longitude,
    string PhotoReference);

public sealed record ReviewFarmerRequest(bool Approved, string? Comment);

public sealed record SupplierDto(Guid Id, string Name, string ContactPhone);

public sealed record PurchaseOrderDto(Guid Id, string PurchaseNumber, Guid SupplierId, Guid InventoryItemId, decimal Quantity, string BatchNumber, bool SubmittedForReceipt);

public sealed record CreateSupplierRequest(string Name, string ContactPhone);

public sealed record CreatePurchaseOrderRequest(string PurchaseNumber, Guid SupplierId, Guid InventoryItemId, decimal Quantity, string BatchNumber);

public sealed record ReceiveStockRequest(Guid InventoryItemId, Guid WarehouseId, string LotCode, decimal Quantity, DateOnly? ExpiresOn);

public sealed record CampaignDto(
    Guid Id,
    string Name,
    string DistrictCode,
    string ValueChain,
    DateOnly StartsOn,
    DateOnly EndsOn,
    decimal SiteLatitude,
    decimal SiteLongitude,
    decimal GpsRadiusMeters,
    string Status);

public sealed record CreateCampaignRequest(
    string Name,
    string DistrictCode,
    string ValueChain,
    DateOnly StartsOn,
    DateOnly EndsOn,
    decimal SiteLatitude,
    decimal SiteLongitude,
    decimal GpsRadiusMeters);

public sealed record AllocationDto(Guid Id, Guid CampaignId, Guid FarmerId, Guid InventoryItemId, decimal AllocatedQuantity, decimal DeliveredQuantity, string AllocationToken);

public sealed record AllocateFarmerRequest(Guid CampaignId, Guid FarmerId, Guid InventoryItemId, decimal AllocatedQuantity);

public sealed record VehicleDto(Guid Id, string Registration, string DriverUserId, bool TrackingActive);

public sealed record RegisterVehicleRequest(string Registration, string DriverUserId, bool TrackingActive);

public sealed record DispatchDto(Guid Id, Guid CampaignId, Guid WarehouseId, string VehicleRegistration, string DriverUserId, string ManifestBarcode, string Status);

public sealed record CreateDispatchRequest(Guid CampaignId, Guid WarehouseId, string VehicleRegistration, string DriverUserId);

public sealed record DistributionSessionDto(Guid Id, Guid CampaignId, Guid DispatchId, string FieldOfficerUserId, decimal Latitude, decimal Longitude, DateTimeOffset StartedAt);

public sealed record StartDistributionSessionRequest(Guid CampaignId, Guid DispatchId, string FieldOfficerUserId, decimal Latitude, decimal Longitude, string ManifestBarcode);

public sealed record ProofOfDeliveryRequest(
    Guid AllocationId,
    Guid DeliveryId,
    string FarmerBarcode,
    string PackageBarcode,
    string OtpCode,
    string FaceCaptureReference,
    decimal Latitude,
    decimal Longitude,
    string VehicleRegistration,
    decimal Quantity,
    string CapturedByUserId,
    DateTimeOffset Timestamp,
    string? SignatureReference,
    string? PhotoEvidenceReference,
    string OfflineTransactionId);

public sealed record ProofOfDeliveryResult(bool Succeeded, string Status, IReadOnlyCollection<string> ValidationMessages);

public sealed record ExceptionCaseDto(Guid Id, string CaseType, string Severity, string Description, string RelatedEntityType, Guid RelatedEntityId, string Status);

public sealed record CreateExceptionCaseRequest(string CaseType, string Severity, string Description, string RelatedEntityType, Guid RelatedEntityId);

public sealed record ResolveExceptionCaseRequest(string Decision);

public sealed record AuditLogDto(Guid Id, string ActorUserId, string Action, string EntityType, Guid? EntityId, string MetadataJson, DateTimeOffset CreatedAt);

public sealed record ReportSummaryDto(
    int FarmersRegistered,
    int FarmersApproved,
    int CampaignsActive,
    int DeliveriesCompleted,
    int OpenExceptions,
    int OfflineMutationsPending);
