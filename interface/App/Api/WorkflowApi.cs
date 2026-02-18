using App.Database;
using Microsoft.AspNetCore.Mvc;

namespace App.Api;

public static class WorkflowApi
{
    public static void MapWorkflowApi(this WebApplication app)
    {
        var group = app.MapGroup("Workflow");

        group.MapPost("/", CreateWorkflow);
    }

    private static async Task<IResult> CreateWorkflow([FromBody] CreateWorkflowDto createWorkflow, ApplicationDataContext db)
    {

        return Results.Ok();
    }

    private record CreateWorkflowDto(string Data);
}