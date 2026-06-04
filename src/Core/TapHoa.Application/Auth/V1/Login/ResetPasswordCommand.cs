using MediatR;

namespace TapHoa.Application.Auth.V1.Login;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest;
