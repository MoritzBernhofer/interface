using Api.Services;

namespace App.Api;

public static class InfoApi
{
    public static void MapInfoEndpoints(this WebApplication app)
    {
        app.MapGet("/post_info", async (WebSocketService wsService, HttpContext context) =>
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = "MAMA MIA";

            if (!wsService.IsConnected)
            {
                return Results.Problem("WebSocket not connected to central server");
            }


            try
            {
                await wsService.SendAsync(body);
                return Results.Ok(new { message = "Info sent to central server" });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to send message: {ex.Message}");
            }
        });
    }
}