using MediatR;

namespace TapHoa.Application.Auth.V1.Register;

public record ConfirmEmailCommand(string Token) : IRequest;
