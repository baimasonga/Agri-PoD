using AgriPod.Application.Security;
using AgriPod.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController(UserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyCollection<UserDto>> List(CancellationToken cancellationToken) =>
        await userService.ListAsync(cancellationToken);

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(List), new { id = user.Id }, user);
    }
}
