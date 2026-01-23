using System.Text;
using central_server.Api.Auth;
using central_server.Api.ClientServer;
using central_server.Api.IotDevice;
using central_server.Api.IotDeviceLog;
using central_server.Api.IotService;
using central_server.Api.User;
using central_server.Api.UserLog;
using central_server.Api.WS;
using central_server.Database;
using central_server.Services.Auth;
using central_server.Services.WS;
using central_server.WS;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//services
builder.Services.AddSingleton<WsClientService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();
builder.Services.AddDbContext<AppDataContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

//endpoints
app.MapWebsocket();
app.MapWsEndpoints();
app.MapAuthEndpoints();
app.MapClientServerEndpoints();
app.MapIotDeviceEndpoints();
app.MapIotDeviceLogEndpoints();
app.MapIotServiceEndpoints();
app.MapUserEndpoints();
app.MapUserLogEndpoints();

app.Run();