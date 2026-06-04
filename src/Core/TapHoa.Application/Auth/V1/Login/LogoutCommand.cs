using MediatR;

namespace TapHoa.Application.Auth.V1.Login;

public record LogoutCommand(string RefreshToken) : IRequest;
