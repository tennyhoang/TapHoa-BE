using MediatR;
using TapHoa.Application.Auth.V1.Login;

namespace TapHoa.Application.Auth.V1.SocialLogin;

/// <param name="Provider">"Google" or "Facebook"</param>
/// <param name="Token">idToken (Google) or accessToken (Facebook)</param>
public record SocialLoginCommand(string Provider, string Token) : IRequest<LoginResponse>;
