namespace Api.Services.Iot;

public class HttpWorkflowRunner(
    JobInfo info,
    HttpWorkflow workflow,
    IHttpClientFactory httpClientFactory,
    CLogger logger)
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
        var url = $"http://{workflow.Ipv4}{workflow.Url}";

        while (!ct.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation($"Job running, {url}");
                var client = httpClientFactory.CreateClient();

                var result = await client.GetAsync(url, ct);
                var content = await result.Content.ReadAsStringAsync(ct);
                //save answer
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                logger.LogWarning($"Job canceled, {url}");
                // normal shutdown
            }
            catch (Exception ex)
            {
                //TODO throw error
                Info.State = JobState.Faulted;
                logger.LogError($"Job failed, {url}", ex);
            }

            if (!await timer.WaitForNextTickAsync(ct)) break;
        }
    }
}