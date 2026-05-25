using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TapHoa.Application.Contracts;
using TapHoa.Infrastructure.Auth;
using TapHoa.Infrastructure.Cloudinary;
using TapHoa.Infrastructure.Payment;

namespace TapHoa.Infrastructure;

public static class InfrastructureExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
            throw new InvalidOperationException(
                "Jwt:Key is missing or too short (minimum 32 characters). " +
                "On Render: set the Jwt__Key environment variable. " +
                "Locally: add it to appsettings.Development.json under \"Jwt\": { \"Key\": \"...\" }.");

        services.AddScoped<IJwtService, JwtService>();

        // Cloudinary — đọc từ appsettings hoặc env vars (Cloudinary__CloudName, ...)
        services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
        services.AddScoped<ICloudinaryService, CloudinaryService>();

        // SePay — đọc từ appsettings hoặc env var SePay__ApiKey
        services.Configure<SepayOptions>(configuration.GetSection(SepayOptions.Section));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = "unique_name"
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin",  policy => policy.RequireRole("Admin"));
            options.AddPolicy("Agent",  policy => policy.RequireRole("Agent"));
            options.AddPolicy("Driver", policy => policy.RequireRole("Driver"));
        });

        return services;
    }
}
