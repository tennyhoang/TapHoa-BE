using System.ComponentModel.DataAnnotations;
using MediatR;

namespace TapHoa.Application.Auth.V1.Register;

public record RegisterCommand(
    [Required, MaxLength(200)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    string? PhoneNumber
) : IRequest<RegisterResponse>;
