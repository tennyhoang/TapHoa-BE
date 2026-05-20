using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TapHoa.Application.Contracts;
using TapHoa.Infrastructure.Auth;

namespace TapHoa.Infrastructure;

public static class InfrastructureExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJwtService, JwtService>();

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
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
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
