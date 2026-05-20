using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Infrastructure.Auth;

namespace TapHoa.Infrastructure.Tests.Auth;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-min-32-characters-long",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();

        _jwtService = new JwtService(config);
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsValidJwt()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Nguyen Van A",
            Email = "test@example.com",
            PasswordHash = "hash"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Subject.Should().Be(user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_TwoDifferentUsers_ReturnsDifferentTokens()
    {
        var user1 = new User { Id = Guid.NewGuid(), FullName = "A", Email = "a@a.com", PasswordHash = "h" };
        var user2 = new User { Id = Guid.NewGuid(), FullName = "B", Email = "b@b.com", PasswordHash = "h" };

        var token1 = _jwtService.GenerateToken(user1);
        var token2 = _jwtService.GenerateToken(user2);

        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateToken_AdminUser_RoleClaimReadableWithRoleClaimTypeRole()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Admin",
            Email = "admin@test.com",
            PasswordHash = "hash",
            Role = UserRole.Admin,
        };

        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var roleClaimsInJwt = jwt.Claims
            .Where(c => c.Value == "Admin")
            .Select(c => c.Type)
            .ToList();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-secret-key-min-32-characters-long"));
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "TestIssuer",
            ValidAudience = "TestAudience",
            IssuerSigningKey = key,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = "unique_name",
        };

        var principal = handler.ValidateToken(token, parameters, out _);

        roleClaimsInJwt.Should().Contain(ClaimTypes.Role, "JWT must expose role claim for Admin policy");
        principal.IsInRole("Admin").Should().BeTrue("Admin authorization policy must accept generated token");
    }
}
