namespace TapHoa.Application.Auth.V1.Login;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string Email,
    string FullName,
    string Role,
    bool EmailConfirmed = false
);
