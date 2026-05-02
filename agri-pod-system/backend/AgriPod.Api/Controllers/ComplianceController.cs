using AgriPod.Application.Compliance;
using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Authorize(Roles = "SystemAdministrator,Auditor,DistrictCoordinator")]
[Route("api/v1/compliance")]
public sealed class ComplianceController(ComplianceService complianceService) : ControllerBase
{
    [HttpGet("exceptions")]
    public async Task<IReadOnlyCollection<ExceptionCaseDto>> Exceptions(CancellationToken cancellationToken) =>
        await complianceService.ExceptionsAsync(cancellationToken);

    [HttpPost("exceptions")]
    public async Task<ActionResult<ExceptionCaseDto>> CreateException(CreateExceptionCaseRequest request, CancellationToken cancellationToken) =>
        Ok(await complianceService.CreateExceptionAsync(request, cancellationToken));

    [HttpPost("exceptions/{id:guid}/resolve")]
    public async Task<ActionResult<ExceptionCaseDto>> Resolve(Guid id, ResolveExceptionCaseRequest request, CancellationToken cancellationToken)
    {
        var exception = await complianceService.ResolveAsync(id, request, cancellationToken);
        return exception is null ? NotFound() : Ok(exception);
    }

    [HttpGet("audit-logs")]
    public async Task<IReadOnlyCollection<AuditLogDto>> AuditLogs(CancellationToken cancellationToken) =>
        await complianceService.AuditLogsAsync(cancellationToken);
}
