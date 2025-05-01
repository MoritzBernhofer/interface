using central_server.Services.WS;

namespace central_server.Api.WS;

public class GetWSClients
{
    public static List<string> HandleGetWSClients(WSClientService wsClientService)
    {
        return wsClientService.ConnectedIds.Select(guid => guid.ToString()).ToList();
    }
}