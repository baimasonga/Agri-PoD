using AgriPod.Application.Campaigns;
using AgriPod.Application.Compliance;
using AgriPod.Application.Deliveries;
using AgriPod.Application.Dispatch;
using AgriPod.Application.Farmers;
using AgriPod.Application.Fleet;
using AgriPod.Application.Inventory;
using AgriPod.Application.Procurement;
using AgriPod.Application.Reports;
using AgriPod.Application.Security;
using AgriPod.Application.Sync;
using Microsoft.Extensions.DependencyInjection;

namespace AgriPod.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<InventoryService>();
        services.AddScoped<DeliveryService>();
        services.AddScoped<ProofOfDeliveryService>();
        services.AddScoped<FleetTrackingService>();
        services.AddScoped<SyncService>();
        services.AddScoped<UserService>();
        services.AddScoped<FarmerService>();
        services.AddScoped<ProcurementService>();
        services.AddScoped<CampaignService>();
        services.AddScoped<DispatchService>();
        services.AddScoped<ComplianceService>();
        services.AddScoped<ReportingService>();
        return services;
    }
}
