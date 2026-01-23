using central_server.Database;
using Microsoft.EntityFrameworkCore;

namespace central_server.Api.UserLog;

public static class UserLogEndpoints
{
    public static void MapUserLogEndpoints(this WebApplication app)
    {
        app.MapGet("/userlogs", HandleGetAll)
            .WithName("GetAllUserLogs")
            .Produces<List<UserLogDto>>();

        app.MapGet("/userlogs/{id}", HandleGetById)
            .WithName("GetUserLogById")
            .Produces<UserLogDto>()
            .Produces(404);
    }

    private static async Task<IResult> HandleGetAll(AppDataContext db)
    {
        var logs = await db.UserLogs
            .Select(l => new UserLogDto(l.Id, l.CreatedAt, l.Content, l.UserId))
            .ToListAsync();
        return Results.Ok(logs);
    }

    private static async Task<IResult> HandleGetById(long id, AppDataContext db)
    {
        var log = await db.UserLogs.FindAsync(id);
        if (log is null)
            return Results.NotFound();
        return Results.Ok(new UserLogDto(log.Id, log.CreatedAt, log.Content, log.UserId));
    }
}
