using Api.Services;
using Api.Services.Iot;
using App.Database;
using Microsoft.EntityFrameworkCore;

namespace App;

public class WorkflowSeeder(ApplicationDataContext db, IotWorkflowManager manager, CLogger logger)
{
    public async Task SeedWorkflows()
    {
        var workflows = await db.IotServices
            .Include(iotService => iotService.IotDevice)
            .ToListAsync();

        foreach (var workflow in workflows)
        {
            var httpWorkflow = new HttpWorkflow()
            {
                Id = workflow.Id,
                Ipv4 = workflow.IotDevice.IPv4,
                Body = workflow.Body,
                SleepTime = workflow.SleepTime,
                Url = workflow.Url,
            };

            logger.LogInformation($"Workflow {workflow.Id} created");

            manager.StartWorkflow(httpWorkflow);
        }
    }
}