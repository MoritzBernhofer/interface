using central_server.Services.WS;

namespace central_server.Api.WS;

public static class WsEndpoints
{
    public static void MapWsEndpoints(this WebApplication app)
    {
        app.MapGet("/wsclient", HandleGetWsClients)
            .WithName("GetWSClients")
            .Produces<List<string>>();

        app.MapPost("/sendMessage", HandleSendMessageToWsClient)
            .WithName("SendMessageToWSClient")
            .Produces<bool>();
    }
    private static async Task<bool> HandleSendMessageToWsClient(SendMessageDto sendMessageDto, WsClientService wsClientService)
    {
        return await wsClientService.SendToAsync(sendMessageDto.Id, sendMessageDto.Message);
    }
    private static List<string> HandleGetWsClients(WsClientService wsClientService)
    {
        return wsClientService.ConnectedIds.Select(guid => guid.ToString()).ToList();
    }
}