using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using App.Database;
using App.Database.Model.UserRelated;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace App.Api;

public static class UserApi
{
    public static void MapUserApi(this WebApplication app)
    {
        var group = app.MapGroup("User");

        group.MapPost("/login", Login);
        group.MapPost("/Register", Register);
    }

    private static async Task<IResult> Login([FromBody] LoginDto login, ApplicationDataContext db)
    {
        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Username == login.Username);
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET")!;

        if (existingUser is null)
        {
            return Results.NotFound("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(login.Password, existingUser.Password))
        {
            return Results.BadRequest("Wrong password");
        }

        var claims = new[]
        {
            new Claim("Username", login.Username),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: cred
        );

        return Results.Ok(new JwtSecurityTokenHandler().WriteToken(token));
    }

    private static async Task<IResult> Register([FromBody] RegisterDto registerDto, ApplicationDataContext db)
    {
        var key = Environment.GetEnvironmentVariable("REGISTER_KEY")!;

        if (key != registerDto.RegisterKey)
        {
            return Results.BadRequest();
        }

        var user = new User()
        {
            Username = registerDto.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Results.Created($"/user/{user.Username}", user.Username);
    }

    private record LoginDto(string Username, string Password);

    private record RegisterDto(string Username, string Password, string RegisterKey);
}