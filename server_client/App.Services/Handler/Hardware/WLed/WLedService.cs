using Api.Services.Attributes;

namespace Api.Services.Handler.Hardware.WLed;

[Service("Wled", "Service for controlling Wled")]
public class WLedService(HttpClient httpClient)
{
    [ServiceMethod("TurnOn", [typeof(string)])]
    public async Task<HttpResponseMessage> TurnOn(string url)
    {
        return await httpClient.GetAsync($"http://{url}/win?T=1");
    }

    [ServiceMethod("TurnOff", [typeof(string)])]
    public async Task<HttpResponseMessage> TurnOff(string url)
    {
        return await httpClient.GetAsync($"http://{url}/win?T=0");
    }
    [ServiceMethod("ChangeColor", [typeof(string), typeof(int), typeof(int), typeof(int)])]
    public async Task<HttpResponseMessage> ChangeColor(string url, int r, int g, int b)
    {
        return await httpClient.GetAsync($"http://{url}/win?T=1&R={r}&G={g}&B={b}");
    }
}