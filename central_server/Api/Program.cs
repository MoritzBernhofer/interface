using central_server.Api.WS;
using central_server.Database;
using central_server.Services.WS;
using central_server.WS;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//env
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//services
builder.Services.AddSingleton<WsClientService>();
builder.Services.AddDbContext<AppDataContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

//endpoints
app.MapWebsocket();
app.MapWsEndpoints();
app.MapGet("/", () => "Server online");

app.Run();