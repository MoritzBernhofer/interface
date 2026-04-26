using System.Security.Claims;
using App.Database;
using Microsoft.AspNetCore.Mvc;

namespace App.Api;

public static class UserApi
{
    public static void MapUserApi(this WebApplication app)
    {
        var group = app.MapGroup("User");

        group.MapPost("/", Login);
    }

    private static async Task<IResult> Login([FromBody] LoginDto login, ApplicationDataContext db)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
        };


        return Results.Ok();
    }

    private static async Task<IResult> Register([FromBody] RegisterDto registerDto, ApplicationDataContext db)
    {

        var key = Environment.GetEnvironmentVariable("REGISTER_KEY")!;

        if()

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
        };


        return Results.Ok();
    }

    private record LoginDto(string Username, string Password);

    private record RegisterDto(string Username, string Password, string RegisterKey);
}