namespace central_server.Logging;

public class CLogger(ILogger<CLogger> logger)
{
    public void LogInformation(string message)
    {
        logger.LogInformation(message);
    }

    public void LogWarning(string message, Exception? ex = null)
    {
        logger.LogWarning(ex, message);
    }

    public void LogError(string message)
    {
        logger.LogInformation(message);
    }
}