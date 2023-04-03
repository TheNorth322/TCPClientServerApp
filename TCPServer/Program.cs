using TCPClientApp.Model;

public static class Program
{
    public static async Task Main()
    {
        await Run();
    }

    private static async Task Run()
    {
        TCPServer tcpServer = new TCPServer();
        await tcpServer.Start();
    }
}