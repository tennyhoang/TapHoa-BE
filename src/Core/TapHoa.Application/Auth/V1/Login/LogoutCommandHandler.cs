using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Auth.V1.Login;

public class LogoutCommandHandler(IRepository<RefreshToken> refreshTokenRepo)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await refreshTokenRepo.FindAsync(rt =>
            rt.Token == request.RefreshToken && !rt.IsRevoked);

        if (storedToken is null)
            return;

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        await refreshTokenRepo.SaveChangesAsync();
    }
}
