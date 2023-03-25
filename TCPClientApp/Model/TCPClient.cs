using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPClientApp.Model;

public class TCPClient
{
    private Socket server;

    public async Task ConnectAsync(string endPoint)
    {
        IPEndPoint ipEndPoint = IPEndPoint.Parse(endPoint);
        Socket socket =
            new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server = socket;
        await server.ConnectAsync(ipEndPoint);
    }

    public async Task<string> SendRequestAsync(string message)
    {
        while (true)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            _ = await server.SendAsync(messageBytes, SocketFlags.None);
            Console.WriteLine($"Socket client sent message: \"{message}\"");

            byte[] buffer = new byte[1_024];
            int received = await server.ReceiveAsync(buffer, SocketFlags.None);
            string response = Encoding.UTF8.GetString(buffer, 0, received);
            if (response.IndexOf("<|EOM|>") > -1)
            {
                response.Replace("<|EOM|>", "");
                Console.WriteLine(
                    $"Socket client received acknowledgment: \"{response}\"");
                return response;
            }
        }
    }

    public void Disconnect()
    {
        server.Shutdown(SocketShutdown.Both);
    }
}