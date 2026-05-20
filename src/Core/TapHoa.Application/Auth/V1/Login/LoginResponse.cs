namespace TapHoa.Application.Auth.V1.Login;

public record LoginResponse(string AccessToken, string Email, string FullName, string Role);
