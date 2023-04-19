using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPClientApp.Domain;

namespace TCPClientApp.Model;

public class TCPClient
{
    public TcpClient _clientSocket;
    private ResponseParser parser;

    public TCPClient()
    {
        parser = new ResponseParser();
    }
    public async Task ConnectAsync(string endPoint)
    {
        IPEndPoint ipEndPoint = IPEndPoint.Parse(endPoint);
        _clientSocket = new TcpClient();
        await GetPort(ipEndPoint);
        _clientSocket = new TcpClient();
        await _clientSocket.ConnectAsync(ipEndPoint);
    }

    private async Task GetPort(IPEndPoint ipEndPoint)
    {
        await _clientSocket.ConnectAsync(ipEndPoint);
        string port = (await GetResponseAsync()).Contents;
        ipEndPoint.Port = Convert.ToInt32(port);
        _clientSocket.Close();
    }

    public async Task<Response> SendRequestAsync(string message)
    {
        NetworkStream networkStream = _clientSocket.GetStream();
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        
        await networkStream.WriteAsync(messageBytes, 0, messageBytes.Length);
        await networkStream.FlushAsync();
        
        return await GetResponseAsync();
    }

    public async Task<Response> GetResponseAsync()
    {
        NetworkStream networkStream = _clientSocket.GetStream();
        byte[] buffer = new byte[1024 * 32];
        StringBuilder response = new StringBuilder();
        int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);

        while (bytesRead > 0)
        {
            response.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

            if (networkStream.DataAvailable == false)
                break;

            bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
        }

        return parser.Parse(response.ToString());
    }

    public void Disconnect() => _clientSocket.Close();
}