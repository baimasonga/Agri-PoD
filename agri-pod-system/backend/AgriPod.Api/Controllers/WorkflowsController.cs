using AgriPod.Application.Workflows;
using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Authorize(Roles = "SystemAdministrator,ProjectManager,DistrictCoordinator,WarehouseManager,Auditor,MonitoringEvaluationOfficer")]
[Route("api/v1/workflows")]
public sealed class WorkflowsController(WorkflowInboxService workflowInboxService) : ControllerBase
{
    [HttpGet("inbox")]
    public async Task<WorkflowInboxDto> Inbox(CancellationToken cancellationToken) =>
        await workflowInboxService.GetAsync(cancellationToken);
}
