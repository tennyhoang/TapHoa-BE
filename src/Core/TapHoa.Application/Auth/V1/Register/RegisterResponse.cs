namespace TapHoa.Application.Auth.V1.Register;

public record RegisterResponse(string AccessToken, string Email, string FullName, string Role);
