using AgriPod.Application.Reports;
using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Authorize(Roles = "SystemAdmin,ProjectManager,DistrictCoordinator,MonitoringOfficer,Auditor,Viewer")]
[Route("api/v1/reports")]
public sealed class ReportsController(ReportingService reportingService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ReportSummaryDto> Summary(CancellationToken cancellationToken) =>
        await reportingService.SummaryAsync(cancellationToken);
}
