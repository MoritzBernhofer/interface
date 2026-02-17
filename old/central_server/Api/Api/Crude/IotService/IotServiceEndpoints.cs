using central_server.Database;
using Microsoft.EntityFrameworkCore;

namespace central_server.Api.IotService;

public static class IotServiceEndpoints
{
    public static void MapIotServiceEndpoints(this WebApplication app)
    {
        app.MapGet("/iotservices", HandleGetAll)
            .WithName("GetAllIotServices")
            .Produces<List<IotServiceDto>>();

        app.MapGet("/iotservices/{id}", HandleGetById)
            .WithName("GetIotServiceById")
            .Produces<IotServiceDto>()
            .Produces(404);

        app.MapPost("/iotservices", HandleCreate)
            .WithName("CreateIotService")
            .Produces<IotServiceDto>();

        app.MapDelete("/iotservices/{id}", HandleDelete)
            .WithName("DeleteIotService")
            .Produces<bool>()
            .Produces(404);
    }

    private static async Task<IResult> HandleGetAll(AppDataContext db)
    {
        var services = await db.IotServices
            .Select(s => new IotServiceDto(s.Id, s.Name))
            .ToListAsync();
        return Results.Ok(services);
    }

    private static async Task<IResult> HandleGetById(long id, AppDataContext db)
    {
        var service = await db.IotServices.FindAsync(id);
        if (service is null)
            return Results.NotFound();
        return Results.Ok(new IotServiceDto(service.Id, service.Name));
    }

    private static async Task<IResult> HandleCreate(CreateIotServiceDto dto, AppDataContext db)
    {
        var service = new central_server.Database.models.IotService
        {
            Name = dto.Name
        };
        db.IotServices.Add(service);
        await db.SaveChangesAsync();
        return Results.Ok(new IotServiceDto(service.Id, service.Name));
    }

    private static async Task<IResult> HandleDelete(long id, AppDataContext db)
    {
        var service = await db.IotServices.FindAsync(id);
        if (service is null)
            return Results.NotFound();
        db.IotServices.Remove(service);
        await db.SaveChangesAsync();
        return Results.Ok(true);
    }
}
