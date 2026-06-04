namespace TapHoa.Application.Auth.V1.Register;

public record RegisterResponse(
    string AccessToken,
    string RefreshToken,
    string Email,
    string FullName,
    string Role,
    bool EmailConfirmed
);
