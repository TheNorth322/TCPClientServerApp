using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPClientApp.Model;

public class TCPServer
{
    public async Task Start()
    {
        TcpListener connectionListener = new TcpListener(IPAddress.Any, 8888);
        
        connectionListener.Start();
        Console.WriteLine(" >> " + "Server Started");

        int counter = 0;
            
        while (true)
        {
            counter++;
            TcpClient clientSocket = await connectionListener.AcceptTcpClientAsync();
            Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started!");
            
            ClientHandler clientHandler = new ClientHandler(clientSocket);
            clientHandler.Start();
        }
        
        connectionListener.Stop();
        Console.WriteLine(" >> " + "exit");
    }
    
}