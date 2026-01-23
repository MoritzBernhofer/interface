using central_server.Services.WS;

namespace central_server.Api.WS;

public class GetWsClients
{
    public static List<string> HandleGetWsClients(WsClientService wsClientService)
    {
        return wsClientService.ConnectedIds.Select(guid => guid.ToString()).ToList();
    }
}