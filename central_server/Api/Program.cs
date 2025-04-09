using central_server.Services.DatabaseService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine(connectionString);

//services
builder.Services.AddDbContext<AppDataContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
