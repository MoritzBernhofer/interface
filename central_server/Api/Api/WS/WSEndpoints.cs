namespace central_server.Api.WS;

public static class WSEndpoints
{
    public static void MapWSEndpoints(this WebApplication app)
    {
        app.MapGet("/wsclient", GetWSClients.HandleGetWSClients)
            .WithName("GetWSClients")
            .Produces<List<string>>();

        app.MapPost("/sendMessage", SendMessageToWSClient.HandleSendMessageToWSClient)
            .WithName("SendMessageToWSClient")
            .Produces<bool>();
    }
}