using Api.Services.Hardware;
using Api.Services.Hardware.WLed;

namespace Api.Services.Handler;

public class RequestServiceHandler(WledServiceHandler wledHandler)
{
    public async Task<string> Handle(RequestDto requestDto)
    {
        switch (requestDto.Service)
        {
            case "Wled":
                await wledHandler.Handle(requestDto);
                break;
        }

        return "TODO return";
    }
}