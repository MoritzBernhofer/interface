using Api.Services.Attributes;
using Microsoft.Extensions.Logging;

namespace Api.Services.Handler.Logic.Logging;

[Service("LoggingService", "Provides logging functionality")]
public class LoggingService(ILogger<LoggingService> logger)
{
    [ServiceMethod("Log", [typeof(string)])]
    public void Log(string message)
    {
        logger.LogInformation(message);
    }
}