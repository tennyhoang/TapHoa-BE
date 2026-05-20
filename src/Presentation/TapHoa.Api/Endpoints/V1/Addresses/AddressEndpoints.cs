using MediatR;
using System.Security.Claims;
using TapHoa.Application.Addresses.V1.AddAddress;
using TapHoa.Application.Addresses.V1.DeleteAddress;
using TapHoa.Application.Addresses.V1.GetMyAddresses;
using TapHoa.Application.Addresses.V1.SetDefaultAddress;

namespace TapHoa.Api.Endpoints.V1.Addresses;

public static class AddressEndpoints
{
    public static void MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/addresses").WithTags("Addresses").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetMyAddressesQuery(GetUserId(user)))));

        group.MapPost("/", async (AddAddressRequest request, ClaimsPrincipal user, IMediator mediator) =>
        {
            var userId = GetUserId(user);
            var command = new AddAddressCommand(userId, request.ReceiverName, request.PhoneNumber,
                request.Province, request.District, request.Ward, request.StreetAddress, request.IsDefault);
            var result = await mediator.Send(command);
            return Results.Created($"/api/v1/addresses/{result.Id}", result);
        });

        group.MapPatch("/{id:guid}/default", async (Guid id, ClaimsPrincipal user, IMediator mediator) =>
            Results.Ok(await mediator.Send(new SetDefaultAddressCommand(id, GetUserId(user)))));

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal user, IMediator mediator) =>
        {
            await mediator.Send(new DeleteAddressCommand(id, GetUserId(user)));
            return Results.NoContent();
        });
    }

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record AddAddressRequest(
    string ReceiverName, string PhoneNumber,
    string Province, string District, string Ward,
    string StreetAddress, bool IsDefault = false);
