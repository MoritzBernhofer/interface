using central_server.Api;
using central_server.Api.WS;
using central_server.Services.DatabaseService;
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
builder.Services.AddSingleton<WSClientService>();
builder.Services.AddDbContext<AppDataContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

//endpoints
app.MapWebsocket();
app.MapWSEndpoints();
app.MapGet("/", () => "Server online");

app.Run();