using MediatR;

namespace TapHoa.Application.Auth.V1.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;
