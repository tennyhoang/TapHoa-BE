using MediatR;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Auth.V1.Register;

public class ResendConfirmationEmailCommandHandler(
    IRepository<User> userRepo,
    IEmailService emailService)
    : IRequestHandler<ResendConfirmationEmailCommand>
{
    public async Task Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepo.FindAsync(u => u.Email == request.Email)
            ?? throw new KeyNotFoundException("Email không tồn tại trong hệ thống.");

        if (user.EmailConfirmed)
            throw new InvalidOperationException("Email đã được xác nhận.");

        user.EmailConfirmationToken = Guid.NewGuid().ToString("N");
        user.EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        await userRepo.SaveChangesAsync();

        await emailService.SendEmailConfirmationAsync(
            user.Email, user.FullName, user.EmailConfirmationToken, cancellationToken);
    }
}
