using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MediatR;
using TapHoa.Application.Auth.V1.Login;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Auth.V1.SocialLogin;

public class SocialLoginCommandHandler(
    IRepository<User> userRepo,
    IRepository<RefreshToken> refreshTokenRepo,
    IJwtService jwtService)
    : IRequestHandler<SocialLoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(SocialLoginCommand request, CancellationToken cancellationToken)
    {
        var (email, fullName, avatarUrl) = request.Provider switch
        {
            "Google"   => await VerifyGoogleAsync(request.Token, cancellationToken),
            "Facebook" => await VerifyFacebookAsync(request.Token, cancellationToken),
            _          => throw new ArgumentException($"Provider không hợp lệ: {request.Provider}")
        };

        var user = await userRepo.FindAsync(u => u.Email == email);

        if (user is null)
        {
            user = new User
            {
                FullName    = fullName,
                Email       = email,
                PasswordHash = string.Empty,   // social-only account has no password
                AvatarUrl   = avatarUrl,
            };
            await userRepo.AddAsync(user);
            await userRepo.SaveChangesAsync();
        }
        else if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa.");
        }

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = jwtService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
        };

        await refreshTokenRepo.AddAsync(refreshTokenEntity);
        await refreshTokenRepo.SaveChangesAsync();

        return new LoginResponse(
            jwtService.GenerateToken(user),
            refreshTokenEntity.Token,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.EmailConfirmed
        );
    }

    // ── Google ────────────────────────────────────────────────────────────────
    // Supports both id_token (mobile, JWT format) and access_token (web OAuth)
    private static async Task<(string email, string fullName, string? avatarUrl)>
        VerifyGoogleAsync(string token, CancellationToken ct)
    {
        using var http = new HttpClient();

        GoogleTokenInfo? info;

        if (token.StartsWith("eyJ", StringComparison.Ordinal))
        {
            // JWT id_token — verify via tokeninfo
            info = await http.GetFromJsonAsync<GoogleTokenInfo>(
                $"https://oauth2.googleapis.com/tokeninfo?id_token={token}", ct);
        }
        else
        {
            // OAuth access_token — fetch user info
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            info = await http.GetFromJsonAsync<GoogleTokenInfo>(
                "https://www.googleapis.com/oauth2/v3/userinfo", ct);
        }

        if (info?.Email is null)
            throw new UnauthorizedAccessException("Xác thực Google thất bại hoặc token đã hết hạn.");

        return (info.Email, info.Name ?? info.Email, info.Picture);
    }

    // ── Facebook ──────────────────────────────────────────────────────────────
    private static async Task<(string email, string fullName, string? avatarUrl)>
        VerifyFacebookAsync(string accessToken, CancellationToken ct)
    {
        using var http = new HttpClient();
        var info = await http.GetFromJsonAsync<FacebookUserInfo>(
            $"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}", ct);

        if (info?.Email is null)
            throw new UnauthorizedAccessException(
                "Xác thực Facebook thất bại. Vui lòng cho phép ứng dụng truy cập email.");

        return (info.Email, info.Name ?? info.Email, null);
    }

    // ── Private DTO records ───────────────────────────────────────────────────
    private sealed class GoogleTokenInfo
    {
        [JsonPropertyName("email")]   public string? Email   { get; init; }
        [JsonPropertyName("name")]    public string? Name    { get; init; }
        [JsonPropertyName("picture")] public string? Picture { get; init; }
    }

    private sealed class FacebookUserInfo
    {
        [JsonPropertyName("id")]    public string? Id    { get; init; }
        [JsonPropertyName("name")]  public string? Name  { get; init; }
        [JsonPropertyName("email")] public string? Email { get; init; }
    }
}
