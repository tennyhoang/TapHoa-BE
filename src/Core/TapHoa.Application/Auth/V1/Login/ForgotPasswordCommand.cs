using MediatR;

namespace TapHoa.Application.Auth.V1.Login;

public record ForgotPasswordCommand(string Email) : IRequest;
