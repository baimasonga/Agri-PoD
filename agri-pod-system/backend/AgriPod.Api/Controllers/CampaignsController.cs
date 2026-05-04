using AgriPod.Application.Campaigns;
using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Authorize(Roles = "SystemAdmin,ProjectManager,DistrictCoordinator")]
[Route("api/v1/campaigns")]
public sealed class CampaignsController(CampaignService campaignService) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyCollection<CampaignDto>> List(CancellationToken cancellationToken) =>
        await campaignService.ListAsync(cancellationToken);

    [HttpPost]
    public async Task<ActionResult<CampaignDto>> Create(CreateCampaignRequest request, CancellationToken cancellationToken) =>
        Ok(await campaignService.CreateAsync(request, cancellationToken));

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<CampaignDto>> Approve(Guid id, CancellationToken cancellationToken)
    {
        var campaign = await campaignService.ApproveAsync(id, cancellationToken);
        return campaign is null ? NotFound() : Ok(campaign);
    }

    [HttpPost("allocations")]
    public async Task<ActionResult<AllocationDto>> Allocate(AllocateFarmerRequest request, CancellationToken cancellationToken) =>
        Ok(await campaignService.AllocateAsync(request, cancellationToken));

    [HttpGet("{campaignId:guid}/allocations")]
    public async Task<IReadOnlyCollection<AllocationDto>> Allocations(Guid campaignId, CancellationToken cancellationToken) =>
        await campaignService.AllocationsAsync(campaignId, cancellationToken);
}
