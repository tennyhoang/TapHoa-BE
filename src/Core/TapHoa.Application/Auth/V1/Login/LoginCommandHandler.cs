using MediatR;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Auth.V1.Login;

public class LoginCommandHandler(
    IRepository<User> userRepo,
    IRepository<RefreshToken> refreshTokenRepo,
    IJwtService jwtService)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepo.FindAsync(u => u.Email == request.Email)
            ?? throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new UnauthorizedAccessException("Tài khoản này đăng nhập bằng mạng xã hội (Google/Facebook).");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa.");

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = jwtService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
        };

        await refreshTokenRepo.AddAsync(refreshTokenEntity);
        await refreshTokenRepo.SaveChangesAsync();

        return new LoginResponse(
            jwtService.GenerateToken(user),
            refreshTokenEntity.Token,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.EmailConfirmed
        );
    }
}
