using central_server.Logging;
using central_server.Services.Iot;

namespace central_server.WS;

public class WsReceiver(CLogger logger, IotServiceHandler iotServiceHandler)
{
    //messageType|content
    public async Task<string> Handle(string message)
    {
        var parts = message.Split('|');

        if (parts.Length <= 1)
        {
            return "Invalid Message";
        }
        
        var messageType = parts[0];

        switch (messageType)
        {
            case "IotService":
                await iotServiceHandler.Handle(parts[1]);
                break;
            case "DataIn":
                await iotServiceHandler.Handle(parts[1]);
                break;
        }
        
        logger.LogInformation("Received WsReceiver " + message);

        return "Handled";
    }
}