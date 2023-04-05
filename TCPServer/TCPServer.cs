using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPClientApp.Model;

public class TCPServer
{
    private ILogger _logger;

    public TCPServer(ILogger logger)
    {
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));
        _logger = logger;
    }
    public async Task Start()
    {
        TcpListener connectionListener = new TcpListener(IPAddress.Any, 8888);
        
        connectionListener.Start();
        _logger.Log(" >> Server Started");

        int counter = 0;
            
        while (true)
        {
            counter++;
            TcpClient clientSocket = await connectionListener.AcceptTcpClientAsync();
            _logger.Log($" >>  Client No: {Convert.ToString(counter)} started!");
            
            ClientHandler clientHandler = new ClientHandler(clientSocket, _logger);
            clientHandler.Start();
        }
        
        connectionListener.Stop();
        Console.WriteLine(" >> " + "exit");
    }
}