namespace Api.Services.Iot;

public class HttpWorkflowRunner(
    JobInfo info,
    HttpWorkflow workflow,
    IHttpClientFactory httpClientFactory,
    WebSocketService webSocketService)
{
    private readonly CancellationTokenSource _cts = new();
    private Task? _loop;

    public JobInfo Info { get; } = info;

    public void Start()
    {
        if (_loop is not null) return;
        _loop = RunAsync(_cts.Token);
    }

    public void Stop()
    {
        Info.State = JobState.Stopped;
        _cts.Cancel();
    }

    private async Task RunAsync(CancellationToken ct)
    {
        Info.State = JobState.Running;

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(workflow.SleepTime));

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var client = httpClientFactory.CreateClient();

                var result = await client.GetAsync(workflow.Url, ct);
                var content = await result.Content.ReadAsStringAsync(ct);
                await webSocketService.SendAsync($"DataIn|{workflow.Id}|{content}", ct);
                //Work
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // normal shutdown
            }
            catch (Exception ex)
            {
                //TODO throw error, to central server
                Info.State = JobState.Faulted;
            }

            if (!await timer.WaitForNextTickAsync(ct)) break;
        }
    }
}