using central_server.Database;
using central_server.Database.models;
using Microsoft.EntityFrameworkCore;

namespace central_server.Api.ClientServer;

public static class ClientServerEndpoints
{
    public static void MapClientServerEndpoints(this WebApplication app)
    {
        app.MapGet("/clientservers", HandleGetAll)
            .WithName("GetAllClientServers")
            .Produces<List<ClientServerDto>>();

        app.MapGet("/clientservers/{id}", HandleGetById)
            .WithName("GetClientServerById")
            .Produces<ClientServerDto>()
            .Produces(404);

    }

    private static async Task<IResult> HandleGetAll(AppDataContext db)
    {
        var clientServers = await db.ClientServers
            .Select(cs => new ClientServerDto(cs.Id, cs.Name))
            .ToListAsync();
        return Results.Ok(clientServers);
    }

    private static async Task<IResult> HandleGetById(long id, AppDataContext db)
    {
        var clientServer = await db.ClientServers.FindAsync(id);
        if (clientServer is null)
            return Results.NotFound();
        return Results.Ok(new ClientServerDto(clientServer.Id, clientServer.Name));
    }
    
}
