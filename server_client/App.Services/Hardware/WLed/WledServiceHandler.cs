using Api.Services.Handler;

namespace Api.Services.Hardware.WLed;

public class WledServiceHandler(WLedService wledService)
{
    public async Task<HttpResponseMessage> Handle(RequestDto requestDto)
    {
        var payload = (WledDto)requestDto.Payload;

        return payload.Instruction switch
        {
            "TurnOff" => await wledService.TurnOff(payload.Ip),
            _ => throw new InvalidOperationException($"Unknown instruction: {payload.Instruction}")
        };
    }
}