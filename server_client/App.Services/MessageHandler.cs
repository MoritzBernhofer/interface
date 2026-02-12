using System.Reflection.Metadata;
using Api.Services.Iot;

namespace Api.Services;

public class MessageHandler(CLogger logger, IotWorkflowManager iotWorkflowManager)
{
    public async Task<string> Handle(string message, WebSocketService webSocketService)
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
                var workflow = new HttpWorkflow()
                {
                    Id = int.Parse(parts[1]),
                    Url = parts[2],
                    Body = parts[3],
                    SleepTime = int.Parse(parts[4])
                };
                iotWorkflowManager.StartWorkflow(workflow, webSocketService);
                break;
            case "DataIn":
                break;
        }

        logger.LogInformation("Received WsReceiver " + message);

        return "Handled";
    }
}