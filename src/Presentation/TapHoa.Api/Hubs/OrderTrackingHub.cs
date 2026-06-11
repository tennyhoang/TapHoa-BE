using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TapHoa.Api.Hubs;

[Authorize]
public class OrderTrackingHub : Hub
{
    private const string AdminGroup = "admins";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

        if (Context.User?.IsInRole("Admin") == true)
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

        if (Context.User?.IsInRole("Admin") == true)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroup);

        await base.OnDisconnectedAsync(exception);
    }
}

public record OrderStatusChangedPayload(string OrderId, string Status);
