using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using central_server.Database.models;
using Microsoft.IdentityModel.Tokens;

namespace central_server.Services.Auth;

public record AuthToken(long UserId, string Email, string Name);

public class AuthService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
{
    public AuthToken? GetCurrentUser()
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.User.Identity?.IsAuthenticated != true)
            return null;

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var emailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value;
        var nameClaim = context.User.FindFirst(ClaimTypes.Name)?.Value;

        if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
            return null;

        return new AuthToken(userId, emailClaim ?? "", nameClaim ?? "");
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        Console.WriteLine("ads");
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
