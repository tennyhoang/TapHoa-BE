using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Auth.V1.Register;

public class ConfirmEmailCommandHandler(IRepository<User> userRepo)
    : IRequestHandler<ConfirmEmailCommand>
{
    public async Task Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepo.FindAsync(u =>
            u.EmailConfirmationToken == request.Token);

        if (user is null)
            throw new KeyNotFoundException("Link xác nhận không hợp lệ.");

        if (user.EmailConfirmed)
            return;

        if (user.EmailConfirmationTokenExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Link xác nhận đã hết hạn. Vui lòng yêu cầu gửi lại email xác nhận.");

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiresAt = null;

        await userRepo.SaveChangesAsync();
    }
}
