using AgriPod.Application.Abstractions;
using AgriPod.Domain.Farmers;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Farmers;

public sealed class FarmerService(IAgriPodDbContext db)
{
    public async Task<IReadOnlyCollection<FarmerDto>> ListAsync(CancellationToken cancellationToken) =>
        (await db.Farmers.ToListAsync(cancellationToken))
            .OrderByDescending(x => x.CreatedAt)
            .Select(ToDto)
            .ToList();

    public async Task<FarmerDto> RegisterAsync(RegisterFarmerRequest request, CancellationToken cancellationToken)
    {
        var farmer = new Farmer(
            request.FullName,
            request.NationalId,
            request.Phone,
            request.DistrictCode,
            request.Chiefdom,
            request.Community,
            request.ValueChain,
            request.Latitude,
            request.Longitude,
            request.PhotoReference);

        db.Farmers.Add(farmer);
        db.AuditLogs.Add(new("mobile-field-officer", "FarmerRegistered", nameof(Farmer), farmer.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(farmer);
    }

    public async Task<FarmerDto?> ReviewAsync(Guid id, ReviewFarmerRequest request, CancellationToken cancellationToken)
    {
        var farmer = await db.Farmers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (farmer is null)
        {
            return null;
        }

        farmer.Review(request.Approved, request.Comment);
        db.AuditLogs.Add(new("district-coordinator", request.Approved ? "FarmerApproved" : "FarmerRejected", nameof(Farmer), farmer.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(farmer);
    }

    private static FarmerDto ToDto(Farmer farmer) =>
        new(farmer.Id, farmer.FullName, farmer.NationalId, farmer.Phone, farmer.DistrictCode, farmer.Chiefdom, farmer.Community, farmer.ValueChain, farmer.Latitude, farmer.Longitude, farmer.PhotoReference, farmer.BarcodeToken, farmer.Status.ToString());
}
