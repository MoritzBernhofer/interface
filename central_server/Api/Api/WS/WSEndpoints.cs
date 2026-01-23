namespace central_server.Api.WS;

public static class WsEndpoints
{
    public static void MapWsEndpoints(this WebApplication app)
    {
        app.MapGet("/wsclient", GetWsClients.HandleGetWsClients)
            .WithName("GetWSClients")
            .Produces<List<string>>();

        app.MapPost("/sendMessage", SendMessageToWsClient.HandleSendMessageToWsClient)
            .WithName("SendMessageToWSClient")
            .Produces<bool>();
    }
}