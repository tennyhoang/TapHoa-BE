using MediatR;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Auth.V1.Login;

public class ForgotPasswordCommandHandler(
    IRepository<User> userRepo,
    IEmailService emailService)
    : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepo.FindAsync(u => u.Email == request.Email);

        if (user is null || !user.IsActive)
            return; // Không tiết lộ email có tồn tại hay không

        user.PasswordResetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        await userRepo.SaveChangesAsync();

        await emailService.SendPasswordResetAsync(
            user.Email, user.FullName, user.PasswordResetToken, cancellationToken);
    }
}
