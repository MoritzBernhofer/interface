using System.ComponentModel.DataAnnotations;
using Api.Services.Iot;
using App.Database;
using App.Database.Model.Iot;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace App.Api;

public static class WorkflowApi
{
    public static void MapWorkflowApi(this WebApplication app)
    {
        // Group: /Workflow
        var group = app.MapGroup("/Workflow")
            .WithTags("Workflow")
            .WithOpenApi(o =>
            {
                o.Summary = "Workflow endpoints";
                o.Description = "Endpoints for creating and managing workflows bound to IoT devices.";
                return o;
            });

        // Sub-group: /Workflow/http
        var httpGroup = group.MapGroup("/http")
            .WithTags("Workflow - HTTP")
            .WithOpenApi(o =>
            {
                o.Summary = "HTTP workflows";
                o.Description = "Endpoints for HTTP-based workflows.";
                return o;
            });

        httpGroup.MapPost("/", CreateHttpWorkflow)
            .WithName("CreateHttpWorkflow") // operationId for client generation
            .WithSummary("Create and start an HTTP workflow")
            .WithDescription(
                "Creates a new HTTP workflow for a given IoT device, persists it, and starts the workflow runner.")
            .Accepts<CreateWorkflowDto>("application/json")
            .Produces<CreateWorkflowCreatedResponse>(StatusCodes.Status201Created)
            .Produces<string>(StatusCodes.Status404NotFound, "text/plain")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(op =>
            {
                op.OperationId = "CreateHttpWorkflow";
                op.Responses["201"].Description = "Workflow created and started.";
                op.Responses["404"].Description = "IoT device not found.";
                op.Responses["400"].Description = "Validation failed.";
                return op;
            });
    }

    private static async Task<IResult> CreateHttpWorkflow(
        [FromBody] CreateWorkflowDto createWorkflow,
        ApplicationDataContext db,
        IotWorkflowManager manager)
    {
        var iotDevice = db.IotDevices.FirstOrDefault(x => x.Id == createWorkflow.IotDeviceId);

        if (iotDevice is null)
        {
            return Results.NotFound("IotDevice not found");
        }

        var workflow = new IotService()
        {
            Name = createWorkflow.Name,
            Url = createWorkflow.Url,
            Body = createWorkflow.Body,
            SleepTime = createWorkflow.SleepTime,
            Type = IotServiceType.Http,
            IotDeviceId = createWorkflow.IotDeviceId
        };

        db.IotServices.Add(workflow);
        await db.SaveChangesAsync();

        var httpWorkflow = new HttpWorkflow()
        {
            Id = workflow.Id,
            Url = workflow.Url,
            Ipv4 = iotDevice.IPv4,
            Body = workflow.Body,
            SleepTime = workflow.SleepTime
        };

        manager.StartWorkflow(httpWorkflow);

        return Results.Created($"/workflows/{workflow.Id}", new CreateWorkflowCreatedResponse(workflow.Id));
    }

    public sealed record CreateWorkflowCreatedResponse(int Id);

    public class CreateWorkflowDto
    {
        /// <summary>Target IoT device ID the workflow is attached to.</summary>
        [Required]
        public int IotDeviceId { get; set; }

        /// <summary>Human-readable workflow name.</summary>
        [Required, StringLength(200)]
        public required string Name { get; set; }

        /// <summary>HTTP endpoint URL the workflow will call.</summary>
        [Required, Url, StringLength(2048)]
        public required string Url { get; set; }

        /// <summary>Optional request body sent to the endpoint (format depends on your workflow runner).</summary>
        [Required]
        public required string Body { get; set; }

        /// <summary>Delay between calls in seconds (or your chosen unit).</summary>
        [Required, Range(0, int.MaxValue)]
        public required int SleepTime { get; set; }
    }
}