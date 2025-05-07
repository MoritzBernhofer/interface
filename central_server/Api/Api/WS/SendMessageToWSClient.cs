using central_server.Services.WS;

namespace central_server.Api.WS;

public static class SendMessageToWSClient
{
    public static async Task<bool> HandleSendMessageToWSClient(SendMessageDto sendMessageDto, WSClientService wsClientService)
    {
        return await wsClientService.SendToAsync(sendMessageDto.Id, sendMessageDto.Message);
    }
    public record SendMessageDto(string Id, string Message);
}