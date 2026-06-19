using MediatR;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Auth.V1.Login;

public class RefreshTokenCommandHandler(
    IRepository<User> userRepo,
    IRepository<RefreshToken> refreshTokenRepo,
    IJwtService jwtService)
    : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (!jwtService.IsValidRefreshTokenFormat(request.RefreshToken))
            throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn.");

        // Lookup by token string only — userId is derived from the stored record, not the token itself
        var storedToken = await refreshTokenRepo.FindAsync(rt =>
            rt.Token == request.RefreshToken && !rt.IsRevoked);

        if (storedToken is null)
            throw new UnauthorizedAccessException("Refresh token không tồn tại hoặc đã bị thu hồi.");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token đã hết hạn.");

        var user = await userRepo.GetByIdAsync(storedToken.UserId)
            ?? throw new UnauthorizedAccessException("Người dùng không tồn tại.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa.");

        // Revoke old token (rotation)
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        refreshTokenRepo.Update(storedToken);

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = jwtService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
        };
        await refreshTokenRepo.AddAsync(newRefreshToken);
        await refreshTokenRepo.SaveChangesAsync();

        return new LoginResponse(
            jwtService.GenerateToken(user),
            newRefreshToken.Token,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.EmailConfirmed
        );
    }
}
