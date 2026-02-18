using Api.Services;
using Api.Services.Iot;
using App;
using App.Api;
using App.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

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


app.MapInfoApi();
app.MapWorkflowApi();
app.MapIotDeviceApi();

app.MapGet("/", () => "Iot");

app.Run();

