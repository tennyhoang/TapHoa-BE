using MediatR;

namespace TapHoa.Application.Auth.V1.Login;

public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResponse>;
