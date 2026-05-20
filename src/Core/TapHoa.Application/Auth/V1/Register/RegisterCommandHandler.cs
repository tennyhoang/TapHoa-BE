using MediatR;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Auth.V1.Register;

public class RegisterCommandHandler(IRepository<User> userRepo, IJwtService jwtService)
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await userRepo.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await userRepo.AddAsync(user);
        await userRepo.SaveChangesAsync();

        return new RegisterResponse(jwtService.GenerateToken(user), user.Email, user.FullName, user.Role.ToString());
    }
}
