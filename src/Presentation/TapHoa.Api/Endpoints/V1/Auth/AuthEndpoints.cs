using MediatR;
using TapHoa.Application.Auth.V1.Login;
using TapHoa.Application.Auth.V1.Register;

namespace TapHoa.Api.Endpoints.V1.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterCommand command, IMediator mediator) =>
            Results.Ok(await mediator.Send(command)));

        group.MapPost("/login", async (LoginCommand command, IMediator mediator) =>
            Results.Ok(await mediator.Send(command)));
    }
}
