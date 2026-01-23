using central_server.Database;
using Microsoft.EntityFrameworkCore;

namespace central_server.Api.User;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("/users", HandleGetAll)
            .WithName("GetAllUsers")
            .Produces<List<UserDto>>();

        app.MapGet("/users/{id}", HandleGetById)
            .WithName("GetUserById")
            .Produces<UserDto>()
            .Produces(404);

        app.MapPost("/users", HandleCreate)
            .WithName("CreateUser")
            .Produces<UserDto>();

        app.MapDelete("/users/{id}", HandleDelete)
            .WithName("DeleteUser")
            .Produces<bool>()
            .Produces(404);
    }

    private static async Task<IResult> HandleGetAll(AppDataContext db)
    {
        var users = await db.Users
            .Select(u => new UserDto(u.Id, u.Name, u.Email))
            .ToListAsync();
        return Results.Ok(users);
    }

    private static async Task<IResult> HandleGetById(long id, AppDataContext db)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return Results.NotFound();
        return Results.Ok(new UserDto(user.Id, user.Name, user.Email));
    }

    private static async Task<IResult> HandleCreate(CreateUserDto dto, AppDataContext db)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = new central_server.Database.models.User
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = hashedPassword
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Results.Ok(new UserDto(user.Id, user.Name, user.Email));
    }

    private static async Task<IResult> HandleDelete(long id, AppDataContext db)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return Results.NotFound();
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return Results.Ok(true);
    }
}
