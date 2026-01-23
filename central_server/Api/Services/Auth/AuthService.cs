using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace central_server.Services.Auth;

public record AuthToken(long UserId, string Email, string Name);

public class AuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public AuthToken? GetCurrentUser()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User.Identity?.IsAuthenticated != true)
            return null;

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var emailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value;
        var nameClaim = context.User.FindFirst(ClaimTypes.Name)?.Value;

        if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
            return null;

        return new AuthToken(userId, emailClaim ?? "", nameClaim ?? "");
    }

    public string GenerateToken(Database.models.User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
