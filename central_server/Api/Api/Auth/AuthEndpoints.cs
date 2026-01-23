using central_server.Database;
using central_server.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace central_server.Api.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/login", HandleLogin)
            .WithName("Login")
            .Produces<LoginResponseDto>()
            .Produces(401);

        app.MapGet("/auth/me", HandleMe)
            .WithName("GetCurrentUser")
            .RequireAuthorization()
            .Produces<LoginResponseDto>()
            .Produces(401);
    }

    private static async Task<IResult> HandleLogin(LoginDto dto, AppDataContext db, AuthService authService)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null)
            return Results.Unauthorized();

        if (!authService.VerifyPassword(dto.Password, user.Password))
            return Results.Unauthorized();

        var token = authService.GenerateToken(user);
        return Results.Ok(new LoginResponseDto(token, user.Id, user.Email, user.Name));
    }

    private static IResult HandleMe(AuthService authService)
    {
        var currentUser = authService.GetCurrentUser();
        if (currentUser is null)
            return Results.Unauthorized();

        return Results.Ok(currentUser);
    }
}
