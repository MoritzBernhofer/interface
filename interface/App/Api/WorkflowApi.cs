using Api.Services.Iot;
using App.Database;
using Microsoft.AspNetCore.Mvc;

namespace App.Api;

public static class WorkflowApi
{
    public static void MapWorkflowApi(this WebApplication app)
    {
        var group = app.MapGroup("Workflow");

        var httpGroup = group.MapGroup("/http");
        httpGroup.MapPost("/", CreateHttpWorkflow);
    }

    private static async Task<IResult> CreateHttpWorkflow([FromBody] CreateWorkflowDto createWorkflow, ApplicationDataContext db)
    {

        var httpWorkflow = new HttpWorkflow()
        {
            Id = createWorkflow.Id,
            Url = createWorkflow.Url,
            Body = createWorkflow.Body,
            SleepTime = createWorkflow.SleepTime
        };



        return Results.Ok();
    }

    private record CreateWorkflowDto(int Id, string Url, string Body, int SleepTime);
}