using MediatR;

namespace TapHoa.Application.Auth.V1.Register;

public record ResendConfirmationEmailCommand(string Email) : IRequest;
