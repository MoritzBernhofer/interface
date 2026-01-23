using central_server.Database;
using Microsoft.EntityFrameworkCore;

namespace central_server.Api.IotDevice;

public static class IotDeviceEndpoints
{
    public static void MapIotDeviceEndpoints(this WebApplication app)
    {
        app.MapGet("/iotdevices", HandleGetAll)
            .WithName("GetAllIotDevices")
            .Produces<List<IotDeviceDto>>();

        app.MapGet("/iotdevices/{id}", HandleGetById)
            .WithName("GetIotDeviceById")
            .Produces<IotDeviceDto>()
            .Produces(404);

        app.MapPost("/iotdevices", HandleCreate)
            .WithName("CreateIotDevice")
            .Produces<IotDeviceDto>();

        app.MapDelete("/iotdevices/{id}", HandleDelete)
            .WithName("DeleteIotDevice")
            .Produces<bool>()
            .Produces(404);
    }

    private static async Task<IResult> HandleGetAll(AppDataContext db)
    {
        var iotDevices = await db.IotDevices
            .Select(d => new IotDeviceDto(d.Id, d.Ipv4, d.IotServiceId, d.Name))
            .ToListAsync();
        return Results.Ok(iotDevices);
    }

    private static async Task<IResult> HandleGetById(long id, AppDataContext db)
    {
        var iotDevice = await db.IotDevices.FindAsync(id);
        if (iotDevice is null)
            return Results.NotFound();
        return Results.Ok(new IotDeviceDto(iotDevice.Id, iotDevice.Ipv4, iotDevice.IotServiceId, iotDevice.Name));
    }

    private static async Task<IResult> HandleCreate(CreateIotDeviceDto dto, AppDataContext db)
    {
        var iotDevice = new central_server.Database.models.IotDevice
        {
            Ipv4 = dto.Ipv4,
            IotServiceId = dto.IotServiceId,
            Name = dto.Name
        };
        db.IotDevices.Add(iotDevice);
        await db.SaveChangesAsync();
        return Results.Ok(new IotDeviceDto(iotDevice.Id, iotDevice.Ipv4, iotDevice.IotServiceId, iotDevice.Name));
    }

    private static async Task<IResult> HandleDelete(long id, AppDataContext db)
    {
        var iotDevice = await db.IotDevices.FindAsync(id);
        if (iotDevice is null)
            return Results.NotFound();
        db.IotDevices.Remove(iotDevice);
        await db.SaveChangesAsync();
        return Results.Ok(true);
    }
}
