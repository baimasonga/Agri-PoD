using AgriPod.Application.Abstractions;
using AgriPod.Domain.Deliveries;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Deliveries;

public sealed class DeliveryService(IAgriPodDbContext db)
{
    public async Task<IReadOnlyCollection<DeliveryDto>> ListAsync(CancellationToken cancellationToken)
    {
        return (await db.Deliveries
            .Include(delivery => delivery.Lines)
            .ToListAsync(cancellationToken))
            .OrderByDescending(delivery => delivery.CreatedAt)
            .Select(ToDto)
            .ToList();
    }

    public async Task<DeliveryDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var delivery = await db.Deliveries
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return delivery is null ? null : ToDto(delivery);
    }

    public async Task<DeliveryDto> CreateAsync(CreateDeliveryRequest request, CancellationToken cancellationToken)
    {
        var delivery = new Delivery(
            request.DeliveryNumber,
            request.OriginWarehouseId,
            request.RecipientName,
            request.RecipientPhone,
            request.DestinationDistrict);

        foreach (var line in request.Lines)
        {
            delivery.AddLine(line.InventoryItemId, line.Quantity, line.Barcode);
        }

        db.Deliveries.Add(delivery);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(delivery);
    }

    public async Task<DeliveryDto?> CompleteAsync(Guid id, CompleteDeliveryRequest request, CancellationToken cancellationToken)
    {
        var delivery = await db.Deliveries
            .Include(x => x.Lines)
            .Include(x => x.ProofEvents)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (delivery is null)
        {
            return null;
        }

        delivery.MarkDelivered(
            request.Latitude,
            request.Longitude,
            request.CapturedByUserId,
            request.BiometricReference,
            request.Timestamp,
            request.SignatureReference,
            request.PhotoEvidenceReference,
            request.OfflineTransactionId);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(delivery);
    }

    private static DeliveryDto ToDto(Delivery delivery) =>
        new(
            delivery.Id,
            delivery.DeliveryNumber,
            delivery.RecipientName,
            delivery.RecipientPhone,
            delivery.DestinationDistrict,
            delivery.Status.ToString(),
            delivery.Lines.Select(line => new DeliveryLineDto(line.InventoryItemId, line.Quantity, line.Barcode)).ToList());
}
