using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Users.V1.Admin;

public class DeleteUserCommandHandler(IRepository<User> userRepo)
    : IRequestHandler<DeleteUserCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == request.RequesterId)
            return Result<bool>.Fail("Không thể xóa tài khoản của chính mình.", "SELF_DELETE");

        var user = await userRepo.FindAsync(u => u.Id == request.UserId);
        if (user is null)
            return Result<bool>.Fail("Không tìm thấy người dùng.", "USER_NOT_FOUND");

        userRepo.Remove(user);
        await userRepo.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }
}
