using Microsoft.Extensions.Logging;

namespace Api.Services;

public class CLogger(ILogger<CLogger> logger)
{
    public void LogInformation(string message)
    {
        logger.LogInformation(message);
    }

    public void LogWarning(string message, Exception? ex = null)
    {
        logger.LogWarning(message, ex);
    }

    public void LogError(string message, Exception? ex = null)
    {
        logger.LogError(message, ex);
    }
}