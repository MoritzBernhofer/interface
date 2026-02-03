namespace central_server.WS;

public class WSReceiver
{
    public void Handle(string content)
    {
        Console.WriteLine(content);
    }
}