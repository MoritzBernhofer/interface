using System.Collections.Concurrent;
using System.ComponentModel;
using Microsoft.Extensions.Hosting;

namespace Api.Services.Iot;

public enum JobState
{
    Pending,
    Running,
    Stopped,
    Faulted
}

public sealed class JobInfo
{
    public int Id { get; init; }
    public JobState State { get; set; } = JobState.Pending;
}

public class IotWorkflowManager(IHttpClientFactory httpClientFactory, CLogger logger) : BackgroundService
{
    private readonly ConcurrentDictionary<int, HttpWorkflowRunner> _runners = new();

    public void StartWorkflow(HttpWorkflow workflow)
    {
        if (workflow.SleepTime <= 0)
            throw new ArgumentOutOfRangeException(nameof(workflow.SleepTime), "Must be > 0.");


        var info = new JobInfo { Id = workflow.Id };
        var runner = new HttpWorkflowRunner(info, workflow, httpClientFactory, logger);

        if (!_runners.TryAdd(workflow.Id, runner))
            throw new InvalidOperationException("ID collision.");

        runner.Start();
    }

    public bool Stop(int id)
    {
        if (!_runners.TryGetValue(id, out var runner)) return false;
        runner.Stop();
        return true;
    }

    public JobInfo? Get(int id) => _runners.TryGetValue(id, out var r) ? r.Info : null;
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var r in _runners.Values) r.Stop();
        return base.StopAsync(cancellationToken);
    }
}

public class Workflow
{
    public required int Id { get; set; }
}

public class HttpWorkflow : Workflow
{
    public required string Ipv4 { get; set; }
    public required string Url { get; set; }
    public required string Body { get; set; }
    public required int SleepTime { get; set; }
}