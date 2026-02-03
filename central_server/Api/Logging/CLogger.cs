namespace central_server.Logging;

public class CLogger(ILogger<CLogger> logger)
{
    public void LogInformation(string message)
    {
        logger.LogInformation(message);
    }

    public void LogError(string message)
    {
        logger.LogInformation(message);
    }
}