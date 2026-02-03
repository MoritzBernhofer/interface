using central_server.Logging;

namespace central_server.WS;

public class WsReceiver(CLogger logger)
{
    public void Handle(string content)
    {
        logger.LogInformation("Received WsReceiver " + content);
    }
}