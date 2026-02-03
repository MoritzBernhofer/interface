namespace central_server.Logging;

public class CLogger
{
    public void LogInformation(string message)
    {
        Console.WriteLine(message);
    }

    public void LogError(string message)
    {
        Console.WriteLine(message);
    }
}