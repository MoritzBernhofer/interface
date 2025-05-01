namespace central_server.Api.WS;

public static class WSEndpoints
{
    public static void MapWSEndpoints(this WebApplication app)
    {
        app.MapGet("", GetWSClients.HandleGetWSClients)
            .WithName("GetWSClients")
            .Produces<List<string>>();

    }
}