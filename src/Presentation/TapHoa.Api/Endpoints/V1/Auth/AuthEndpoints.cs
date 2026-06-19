using MediatR;
using Microsoft.AspNetCore.Hosting;
using TapHoa.Application.Auth.V1.Login;
using TapHoa.Application.Auth.V1.Register;
using TapHoa.Application.Auth.V1.SocialLogin;

namespace TapHoa.Api.Endpoints.V1.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth").RequireRateLimiting("AuthPolicy");

        group.MapPost("/register", async (
            RegisterCommand command,
            IMediator mediator,
            HttpResponse response,
            IWebHostEnvironment env) =>
        {
            var result = await mediator.Send(command);
            SetAuthCookies(response, result.AccessToken, result.RefreshToken, env.IsProduction());
            return Results.Ok(new { result.Email, result.FullName, result.Role, result.EmailConfirmed });
        });

        group.MapPost("/login", async (
            LoginCommand command,
            IMediator mediator,
            HttpResponse response,
            IWebHostEnvironment env) =>
        {
            var result = await mediator.Send(command);
            SetAuthCookies(response, result.AccessToken, result.RefreshToken, env.IsProduction());
            return Results.Ok(new { result.Email, result.FullName, result.Role, result.EmailConfirmed });
        });

        group.MapPost("/social-login", async (
            SocialLoginCommand command,
            IMediator mediator,
            HttpResponse response,
            IWebHostEnvironment env) =>
        {
            var result = await mediator.Send(command);
            SetAuthCookies(response, result.AccessToken, result.RefreshToken, env.IsProduction());
            return Results.Ok(new { result.Email, result.FullName, result.Role, result.EmailConfirmed });
        });

        group.MapPost("/refresh-token", async (
            HttpRequest request,
            HttpResponse response,
            IMediator mediator,
            IWebHostEnvironment env) =>
        {
            var refreshToken = request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return Results.Unauthorized();
            var result = await mediator.Send(new RefreshTokenCommand(refreshToken));
            SetAuthCookies(response, result.AccessToken, result.RefreshToken, env.IsProduction());
            return Results.Ok(new { result.Email, result.FullName, result.Role });
        });

        group.MapPost("/logout", async (
            HttpRequest request,
            HttpResponse response,
            IMediator mediator,
            IWebHostEnvironment env) =>
        {
            var refreshToken = request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshToken))
                await mediator.Send(new LogoutCommand(refreshToken));
            ClearAuthCookies(response, env.IsProduction());
            return Results.Ok(new { message = "Đăng xuất thành công." });
        });

        group.MapPost("/confirm-email", async (ConfirmEmailCommand command, IMediator mediator) =>
        {
            await mediator.Send(command);
            return Results.Ok(new { message = "Email đã được xác nhận thành công." });
        });

        group.MapPost("/resend-confirmation", async (ResendConfirmationEmailCommand command, IMediator mediator) =>
        {
            await mediator.Send(command);
            return Results.Ok(new { message = "Email xác nhận đã được gửi lại." });
        });

        group.MapPost("/forgot-password", async (ForgotPasswordCommand command, IMediator mediator) =>
        {
            await mediator.Send(command);
            return Results.Ok(new { message = "Nếu email tồn tại, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu." });
        });

        group.MapPost("/reset-password", async (ResetPasswordCommand command, IMediator mediator) =>
        {
            await mediator.Send(command);
            return Results.Ok(new { message = "Mật khẩu đã được đặt lại thành công." });
        });
    }

    private static void SetAuthCookies(
        HttpResponse response,
        string accessToken,
        string refreshToken,
        bool isProduction)
    {
        var sameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax;

        response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure   = isProduction,
            SameSite = sameSite,
            Path     = "/",
            MaxAge   = TimeSpan.FromMinutes(15),
        });

        response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure   = isProduction,
            SameSite = sameSite,
            Path     = "/api/v1/auth",
            MaxAge   = TimeSpan.FromDays(30),
        });
    }

    private static void ClearAuthCookies(HttpResponse response, bool isProduction)
    {
        var sameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax;

        response.Cookies.Append("access_token", "", new CookieOptions
        {
            HttpOnly = true,
            Secure   = isProduction,
            SameSite = sameSite,
            Path     = "/",
            MaxAge   = TimeSpan.Zero,
        });

        response.Cookies.Append("refresh_token", "", new CookieOptions
        {
            HttpOnly = true,
            Secure   = isProduction,
            SameSite = sameSite,
            Path     = "/api/v1/auth",
            MaxAge   = TimeSpan.Zero,
        });
    }
}
