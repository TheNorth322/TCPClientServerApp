using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPClientApp.Domain;

namespace TCPClientApp.Model;

public class TCPClient
{
    private TcpClient _clientSocket;
    private NetworkStream _networkStream;
    private byte directoryTreeRequest = 1;
    private byte fileContentsRequest = 2;
    private byte fileNameRequest = 3;
    private byte exceptionRequest = 4;
    private byte systemRequest = 5;
    private byte portRequest = 6;
    private byte disconnectRequest = 8;

    public async Task<string> ConnectAsync(string endPoint)
    {
        IPEndPoint ipEndPoint = IPEndPoint.Parse(endPoint);
        _clientSocket = new TcpClient();

        await _clientSocket.ConnectAsync(ipEndPoint);
        _networkStream = _clientSocket.GetStream();
        await GetPortAsync(ipEndPoint);
        _clientSocket.Close();

        _clientSocket = new TcpClient();
        await _clientSocket.ConnectAsync(ipEndPoint);
        _networkStream = _clientSocket.GetStream();
        return ipEndPoint.ToString();
    }

    private async Task GetPortAsync(IPEndPoint ipEndPoint)
    {
        Response response = await GetResponseAsync("");
        ipEndPoint.Port = Convert.ToInt32(response.Contents);
    }

    public async Task<Response> SendRequestAsync(string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        await _networkStream.WriteAsync(messageBytes, 0, messageBytes.Length);
        await _networkStream.FlushAsync();

        return await GetResponseAsync(message);
    }


    private async Task<Response> GetResponseAsync(string message)
    {
        byte[] buffer = new byte[1024 * 8];
        int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);

        switch (buffer[0])
        {
            case 1:
                return new Response(ResponseType.DirectoryContents,
                    await GetStringResponseAsync(buffer, bytesRead));
            case 2:
                return new Response(ResponseType.FileContents,
                    await SaveFileAsync(buffer, bytesRead, message));
            case 3:
                return new Response(ResponseType.FileName,
                    await GetStringResponseAsync(buffer, bytesRead));
            case 4:
                return new Response(ResponseType.Exception,
                    await GetStringResponseAsync(buffer, bytesRead));
            case 5:
                return new Response(ResponseType.System,
                    await GetStringResponseAsync(buffer, bytesRead));
            case 6:
                return new Response(ResponseType.Port,
                    await GetStringResponseAsync(buffer, bytesRead));
            case 7:
                return new Response(ResponseType.Disks,
                    await GetStringResponseAsync(buffer, bytesRead));
            default:
                throw new ApplicationException("Invalid signature!");
        }
    }

    private async Task<string> SaveFileAsync(byte[] buffer, int bytesRead, string request)
    {
        buffer = buffer.Skip(1).ToArray();
        string path = $"C:\\TCPClient\\Downloads\\{Path.GetFileName(request)}";
        FileStream fileStream =
            new FileStream(path, FileMode.Create);
        
        while (bytesRead > 0)
        {
            await fileStream.WriteAsync(buffer);
            await fileStream.FlushAsync();
            if (_networkStream.DataAvailable == false)
                break;

            bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
        }
        fileStream.Close();
        return $"File saved in {path}";
    }

    private async Task<string> GetStringResponseAsync(byte[] buffer, int bytesRead)
    {
        StringBuilder response = new StringBuilder();
        buffer = buffer.Skip(1).ToArray();
        response.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead - 1));
        
        while (bytesRead > 0)
        {
            if (_networkStream.DataAvailable == false)
                break;
            
            bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
            response.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
        }

        return response.ToString();
    }

    public async Task DisconnectAsync()
    {
        NetworkStream networkStream = _clientSocket.GetStream();
        await networkStream.WriteAsync(new[] { disconnectRequest }, 0, 1);
        await networkStream.FlushAsync();
        _clientSocket.Close();
    }
}