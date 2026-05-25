using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Users.V1.Admin;

public record GetAllUsersQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Role
) : IRequest<Result<PagedResult<AdminUserResponse>>>;
