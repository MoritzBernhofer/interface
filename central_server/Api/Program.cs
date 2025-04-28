using central_server.Services.DatabaseService;
using central_server.Services.Grpc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine($"DB Connection: {connectionString}");

// Add EF context
builder.Services.AddDbContext<AppDataContext>(options =>
    options.UseSqlite(connectionString));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000, o => { o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1; });

    options.ListenLocalhost(5001, o => { o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; });
});

var app = builder.Build();

// Map gRPC
app.MapGrpcService<DispatcherService>();

app.MapGet("/test", async () =>
{
    var content = $"Test message at {DateTime.UtcNow:O}";
    await DispatcherService.BroadcastMessageAsync(content);
    return Results.Ok($"Broadcasted: {content}");
});

app.MapGet("/", () => "Server online");

app.Run();