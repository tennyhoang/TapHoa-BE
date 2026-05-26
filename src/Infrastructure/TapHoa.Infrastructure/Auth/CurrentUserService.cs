using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TapHoa.Application.Contracts;

namespace TapHoa.Infrastructure.Auth;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal =>
        httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var raw = Principal?.FindFirstValue("sub")
                   ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated is true;
}
