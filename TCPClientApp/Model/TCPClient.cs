using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPClientApp.Model;

public class TCPClient : IClient
{
    private string _ip;
    private string _port;
    private Socket client;

    public async Task ConnectAsync(string ip, string port)
    {
        IPEndPoint ipEndPoint = IPEndPoint.Parse($"{ip}:{port}");
        using Socket client =
            new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await client.ConnectAsync(ipEndPoint);
    }

    public async Task<string> SendRequestAsync(string message)
    {
        int bytes;
        byte[] messageBytes = Encoding.UTF8.GetBytes(message), buffer = new byte[1024];
        StringBuilder stringBuilder = new StringBuilder();
        
        _ = await client.SendAsync(messageBytes, SocketFlags.None);
        
        do
        {
            bytes = await client.ReceiveAsync(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, bytes);
            stringBuilder.Append(response);
        } while (bytes > 0);

        return stringBuilder.ToString();
    }

    public void Disconnect()
    {
        client.Shutdown(SocketShutdown.Both);
    }
}