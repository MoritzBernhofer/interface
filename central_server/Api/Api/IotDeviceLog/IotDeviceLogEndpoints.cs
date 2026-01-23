using central_server.Database;
using Microsoft.EntityFrameworkCore;

namespace central_server.Api.IotDeviceLog;

public static class IotDeviceLogEndpoints
{
    public static void MapIotDeviceLogEndpoints(this WebApplication app)
    {
        app.MapGet("/iotdevicelogs", HandleGetAll)
            .WithName("GetAllIotDeviceLogs")
            .Produces<List<IotDeviceLogDto>>();
    }

    private static async Task<IResult> HandleGetAll(AppDataContext db)
    {
        var logs = await db.IotDeviceLogs
            .Select(l => new IotDeviceLogDto(l.Id, l.CreatedAt, l.Content, l.IotDeviceId))
            .ToListAsync();
        return Results.Ok(logs);
    }
}
