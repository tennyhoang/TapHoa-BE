using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Auth.V1.Login;

public class ResetPasswordCommandHandler(IRepository<User> userRepo)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepo.FindAsync(u =>
            u.PasswordResetToken == request.Token);

        if (user is null)
            throw new KeyNotFoundException("Link đặt lại mật khẩu không hợp lệ.");

        if (user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Link đặt lại mật khẩu đã hết hạn. Vui lòng yêu cầu gửi lại email.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;

        await userRepo.SaveChangesAsync();
    }
}
