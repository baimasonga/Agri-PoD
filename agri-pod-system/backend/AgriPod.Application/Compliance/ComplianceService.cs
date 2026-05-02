using AgriPod.Application.Abstractions;
using AgriPod.Domain.Compliance;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Compliance;

public sealed class ComplianceService(IAgriPodDbContext db)
{
    public async Task<IReadOnlyCollection<ExceptionCaseDto>> ExceptionsAsync(CancellationToken cancellationToken) =>
        (await db.ExceptionCases.ToListAsync(cancellationToken))
            .OrderByDescending(x => x.CreatedAt)
            .Select(ToDto)
            .ToList();

    public async Task<ExceptionCaseDto> CreateExceptionAsync(CreateExceptionCaseRequest request, CancellationToken cancellationToken)
    {
        var exception = new ExceptionCase(request.CaseType, request.Severity, request.Description, request.RelatedEntityType, request.RelatedEntityId);
        db.ExceptionCases.Add(exception);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(exception);
    }

    public async Task<ExceptionCaseDto?> ResolveAsync(Guid id, ResolveExceptionCaseRequest request, CancellationToken cancellationToken)
    {
        var exception = await db.ExceptionCases.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (exception is null)
        {
            return null;
        }

        exception.Resolve(request.Decision);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(exception);
    }

    public async Task<IReadOnlyCollection<AuditLogDto>> AuditLogsAsync(CancellationToken cancellationToken) =>
        (await db.AuditLogs.ToListAsync(cancellationToken))
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .Select(x => new AuditLogDto(x.Id, x.ActorUserId, x.Action, x.EntityType, x.EntityId, x.MetadataJson, x.CreatedAt))
            .ToList();

    private static ExceptionCaseDto ToDto(ExceptionCase exception) =>
        new(exception.Id, exception.CaseType, exception.Severity, exception.Description, exception.RelatedEntityType, exception.RelatedEntityId, exception.Status);
}
