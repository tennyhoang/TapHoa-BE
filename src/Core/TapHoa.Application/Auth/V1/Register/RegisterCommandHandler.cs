using MediatR;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Auth.V1.Register;

public class RegisterCommandHandler(
    IRepository<User> userRepo,
    IRepository<RefreshToken> refreshTokenRepo,
    IJwtService jwtService,
    IEmailService emailService)
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await userRepo.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var confirmationToken = Guid.NewGuid().ToString("N");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            EmailConfirmationToken = confirmationToken,
            EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        await userRepo.AddAsync(user);
        await userRepo.SaveChangesAsync();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = jwtService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
        };

        await refreshTokenRepo.AddAsync(refreshTokenEntity);
        await refreshTokenRepo.SaveChangesAsync();

        // Fire-and-forget email confirmation — không block response
        _ = emailService.SendEmailConfirmationAsync(user.Email, user.FullName, confirmationToken, cancellationToken);

        return new RegisterResponse(
            jwtService.GenerateToken(user),
            refreshTokenEntity.Token,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.EmailConfirmed
        );
    }
}
