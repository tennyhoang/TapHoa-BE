using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Notifications.V1.RegisterPushToken;

public class RegisterPushTokenCommandHandler(IRepository<PushToken> tokenRepo)
    : IRequestHandler<RegisterPushTokenCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RegisterPushTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await tokenRepo.Query()
            .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

        if (existing is not null)
        {
            if (existing.UserId != request.UserId)
                existing.UserId = request.UserId;
            await tokenRepo.SaveChangesAsync();
            return Result<bool>.Ok(true);
        }

        await tokenRepo.AddAsync(new PushToken
        {
            UserId   = request.UserId,
            Token    = request.Token,
            Platform = request.Platform,
        });
        await tokenRepo.SaveChangesAsync();
        return Result<bool>.Ok(true);
    }
}
