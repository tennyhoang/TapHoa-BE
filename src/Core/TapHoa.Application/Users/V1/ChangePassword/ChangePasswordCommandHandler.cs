using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Users.V1.ChangePassword;

public class ChangePasswordCommandHandler(IRepository<User> userRepo)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepo.GetByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ArgumentException("Mật khẩu hiện tại không đúng.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        userRepo.Update(user);
        await userRepo.SaveChangesAsync();
    }
}
