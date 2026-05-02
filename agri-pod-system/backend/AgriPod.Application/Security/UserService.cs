using AgriPod.Application.Abstractions;
using AgriPod.Domain.Security;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Security;

public sealed class UserService(IAgriPodDbContext db)
{
    public async Task<IReadOnlyCollection<UserDto>> ListAsync(CancellationToken cancellationToken) =>
        await db.AppUsers
            .OrderBy(x => x.Role)
            .ThenBy(x => x.FullName)
            .Select(x => new UserDto(x.Id, x.FullName, x.Email, x.Phone, x.Role.ToString(), x.DistrictCode, x.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var role = Enum.Parse<SystemRole>(request.Role, ignoreCase: true);
        var user = new AppUser(request.FullName, request.Email, request.Phone, role, request.DistrictCode);
        db.AppUsers.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return new UserDto(user.Id, user.FullName, user.Email, user.Phone, user.Role.ToString(), user.DistrictCode, user.IsActive);
    }
}
