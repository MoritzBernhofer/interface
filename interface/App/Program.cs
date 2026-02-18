using Api.Services;
using Api.Services.Iot;
using App.Api;
using App.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Services
builder.Services.AddSingleton<CLogger>();
builder.Services.AddSingleton<IotWorkflowManager>();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<ApplicationDataContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddHostedService(sp => sp.GetRequiredService<IotWorkflowManager>());

var app = builder.Build();

app.MapInfoApi();
app.MapWorkflowApi();

app.Run();