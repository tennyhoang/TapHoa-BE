using MediatR;
using TapHoa.Application.Auth.V1.Login;
using TapHoa.Application.Auth.V1.Register;
using TapHoa.Application.Auth.V1.SocialLogin;

namespace TapHoa.Api.Endpoints.V1.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth").RequireRateLimiting("AuthPolicy");

        group.MapPost("/register", async (RegisterCommand command, IMediator mediator) =>
            Results.Ok(await mediator.Send(command)));

        group.MapPost("/login", async (LoginCommand command, IMediator mediator) =>
            Results.Ok(await mediator.Send(command)));

        group.MapPost("/social-login", async (SocialLoginCommand command, IMediator mediator) =>
            Results.Ok(await mediator.Send(command)));

        group.MapPost("/refresh-token", async (RefreshTokenCommand command, IMediator mediator) =>
            Results.Ok(await mediator.Send(command)));

        group.MapPost("/logout", async (LogoutCommand command, IMediator mediator) =>
        {
            await mediator.Send(command);
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
}
