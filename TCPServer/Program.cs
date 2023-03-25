using TCPClientApp.Model;

public static class Program
{
    public static void Main()
    {
        Run();
    }

    private static async void Run()
    {
        TCPServer tcpServer = new TCPServer();
        while (true)
            await tcpServer.Connect();
    }
}