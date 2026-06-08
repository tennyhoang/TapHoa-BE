using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Infrastructure.Notifications;

public class ExpoPushService(
    IHttpClientFactory httpClientFactory,
    IRepository<PushToken> tokenRepo,
    ILogger<ExpoPushService> logger)
    : IExpoPushService
{
    private const string ExpoEndpoint = "https://exp.host/--/api/v2/push/send";

    public async Task SendAsync(Guid userId, string title, string body, object? data = null, CancellationToken cancellationToken = default)
    {
        var tokens = await tokenRepo.Query()
            .Where(t => t.UserId == userId)
            .Select(t => t.Token)
            .ToListAsync(cancellationToken);

        if (tokens.Count == 0)
            return;

        var messages = tokens.Select(token => new
        {
            to     = token,
            title,
            body,
            data,
            sound  = "default",
        });

        var client = httpClientFactory.CreateClient();
        try
        {
            var response = await client.PostAsJsonAsync(ExpoEndpoint, messages, cancellationToken);
            if (!response.IsSuccessStatusCode)
                logger.LogWarning("Expo push failed for user {UserId}: {Status}", userId, response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending Expo push notification to user {UserId}", userId);
        }
    }
}
