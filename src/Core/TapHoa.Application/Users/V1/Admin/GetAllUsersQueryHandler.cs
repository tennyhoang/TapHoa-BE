using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Users.V1.Admin;

public class GetAllUsersQueryHandler(IRepository<User> userRepo)
    : IRequestHandler<GetAllUsersQuery, Result<PagedResult<AdminUserResponse>>>
{
    public async Task<Result<PagedResult<AdminUserResponse>>> Handle(
        GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = userRepo.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(s) ||
                u.Email.ToLower().Contains(s) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(request.Role) &&
            Enum.TryParse<Domain.Enums.UserRole>(request.Role, out var role))
        {
            query = query.Where(u => u.Role == role);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new AdminUserResponse(
                u.Id, u.FullName, u.Email, u.PhoneNumber,
                u.Role.ToString(), u.IsActive, u.AgentHubId, u.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<AdminUserResponse>>.Ok(new PagedResult<AdminUserResponse>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
        });
    }
}
