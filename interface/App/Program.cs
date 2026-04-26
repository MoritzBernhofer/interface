using System.Text;
using Api.Services;
using Api.Services.Iot;
using App;
using App.Api;
using App.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//secret
DotNetEnv.Env.Load();
var secret = Environment.GetEnvironmentVariable("JWT_SECRET");

// Auth

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!))
        };
    });
builder.Services.AddAuthorization();

// Services
builder.Services.AddSingleton<CLogger>();
builder.Services.AddSingleton<IotWorkflowManager>();
builder.Services.AddScoped<WorkflowSeeder>();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<ApplicationDataContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddHostedService(sp => sp.GetRequiredService<IotWorkflowManager>());

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo()
        {
            Title = "IoT Workflow API",
            Version = "v1",
            Description = "Minimal API for workflow management"
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

//middleware

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<WorkflowSeeder>();
    await seeder.SeedWorkflows();
}

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "IoT Workflow API v1");
        options.RoutePrefix = "swagger";
    });
}


//API
app.MapInfoApi();
app.MapWorkflowApi();
app.MapIotDeviceApi();
app.MapUserApi();

app.MapGet("/", () => "Iot");

app.Run();