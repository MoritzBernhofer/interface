using central_server.Services.WS;

namespace central_server.Api.WS;

public static class SendMessageToWsClient
{
    public static async Task<bool> HandleSendMessageToWsClient(SendMessageDto sendMessageDto, WsClientService wsClientService)
    {
        return await wsClientService.SendToAsync(sendMessageDto.Id, sendMessageDto.Message);
    }
    public record SendMessageDto(string Id, string Message);
}