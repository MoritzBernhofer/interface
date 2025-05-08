namespace Api.Services.Hardware;

public class WLedService(HttpClient httpClient)
{
    public async Task<HttpResponseMessage> TurnOn(string url)
    {
        return await httpClient.GetAsync($"http://{url}/win?T=1");
    }

    public async Task<HttpResponseMessage> TurnOff(string url)
    {
        return await httpClient.GetAsync($"http://{url}/win?T=0");
    }

    public async Task<HttpResponseMessage> ChangeColor(string url, int r, int g, int b)
    {
        return await httpClient.GetAsync($"http://{url}/win?T=1&R={r}&G={g}&B={b}");
    }
}