using WebApplication1.Websocket;

var builder = WebApplication.CreateBuilder(args);

//services


var app = builder.Build();
app.UseWebSockets();
app.MapWSEndpoints();


app.Run();