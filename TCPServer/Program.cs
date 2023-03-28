using TCPClientApp.Model;

public static class Program
{
    public static void Main()
    {
        Run();
    }

    private static void Run()
    {
        TCPServer tcpServer = new TCPServer();
        while (true)
           tcpServer.Connect();
    }
}