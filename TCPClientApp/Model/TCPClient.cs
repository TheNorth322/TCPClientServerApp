using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPClientApp.Model;

public class TCPClient
{
    private TcpClient _clientSocket;

    public TCPClient()
    {
    }

    public bool Connected()
    {
        if (_clientSocket == null || _clientSocket.Connected)
            return false;
        return true;
    }

    public async Task ConnectAsync(string endPoint)
    {
        IPEndPoint ipEndPoint = IPEndPoint.Parse(endPoint);
        _clientSocket = new TcpClient();
        await _clientSocket.ConnectAsync(ipEndPoint);
    }

    public async Task<string> SendRequestAsync(string message)
    {
        NetworkStream networkStream = _clientSocket.GetStream();
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        await networkStream.WriteAsync(messageBytes, 0, messageBytes.Length);
        await networkStream.FlushAsync();
        Console.WriteLine($"Socket client sent message: \"{message}\"");

        byte[] buffer = new byte[1_024];
        StringBuilder response = new StringBuilder();
        int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
        
        while (bytesRead > 0)
        {
            response.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

            if (networkStream.DataAvailable == false)
                break;

            bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
        }

        Console.WriteLine(
            $"Socket client received acknowledgment: \"{response}\"");
        return response.ToString();
    }

    public void Disconnect() => _clientSocket.Close();
}