using System.Security.Claims;
using MediatR;
using TapHoa.Application.Cart.V1.AddToCart;
using TapHoa.Application.Cart.V1.ClearCart;
using TapHoa.Application.Cart.V1.GetCart;
using TapHoa.Application.Cart.V1.RemoveFromCart;
using TapHoa.Application.Cart.V1.UpdateCart;

namespace TapHoa.Api.Endpoints.V1.Cart;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/cart").WithTags("Cart").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetCartQuery(GetUserId(user)))));

        group.MapPost("/", async (AddToCartRequest body, ClaimsPrincipal user, IMediator mediator) =>
            Results.Ok(await mediator.Send(new AddToCartCommand(GetUserId(user), body.ProductId, body.Quantity))));

        group.MapPut("/{productId:guid}", async (Guid productId, UpdateQuantityRequest body, ClaimsPrincipal user, IMediator mediator) =>
            Results.Ok(await mediator.Send(new UpdateCartCommand(GetUserId(user), productId, body.Quantity))));

        group.MapDelete("/{productId:guid}", async (Guid productId, ClaimsPrincipal user, IMediator mediator) =>
            Results.Ok(await mediator.Send(new RemoveFromCartCommand(GetUserId(user), productId))));

        group.MapDelete("/", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            await mediator.Send(new ClearCartCommand(GetUserId(user)));
            return Results.NoContent();
        });
    }

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record AddToCartRequest(Guid ProductId, int Quantity);
public record UpdateQuantityRequest(int Quantity);
