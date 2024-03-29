﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPClientApp.Domain;

namespace TCPClientApp.Model;

public class TCPClient
{
    public IPEndPoint IpEndPoint;
    private TcpClient _clientSocket;
    private Thread ping;
    private NetworkStream _networkStream;
    private int lastPingTime;
    private int pingTime = 5;

    public TCPClient()
    {
        lastPingTime = DateTime.Now.Second;
    }
    
    public async Task ConnectAsync(string endPoint)
    {
        IpEndPoint = IPEndPoint.Parse(endPoint);
        _clientSocket = new TcpClient();

        await _clientSocket.ConnectAsync(IpEndPoint);
        _networkStream = _clientSocket.GetStream();
        await GetPortAsync(IpEndPoint);
        _clientSocket.Close();

        _clientSocket = new TcpClient();
        await _clientSocket.ConnectAsync(IpEndPoint);
        _networkStream = _clientSocket.GetStream();
        ping = new Thread(WaitForPing);
        ping.Start();
    }

    private async void WaitForPing()
    {
        while (true)
        {
            if (Math.Abs(DateTime.Now.Second - lastPingTime) >= pingTime)
            {
                lastPingTime = DateTime.Now.Second;
                if (!await PingAsync())
                    break;
            }
        } 
    }

    private async Task<bool> PingAsync()
    {
        try
        {
            byte[] request = new[] { (byte) RequestType.Ping };
            await _networkStream.WriteAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            CloseSocket();
            return false;
        }
    }
    
    private async Task GetPortAsync(IPEndPoint ipEndPoint)
    {
        Response response = await GetResponseAsync("");
        int.TryParse(response.Contents, out var port);
        ipEndPoint.Port = port;
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

        switch ((RequestType) buffer[0])
        {
            case RequestType.DirectoryContents:
                return new Response(RequestType.DirectoryContents,
                    await GetStringResponseAsync(buffer, bytesRead));
            case RequestType.FileContents:
                return new Response(RequestType.FileContents,
                    await SaveFileAsync(buffer, bytesRead, message));
            case RequestType.Exception:
                return new Response(RequestType.Exception,
                    await GetStringResponseAsync(buffer, bytesRead));
            case RequestType.Port:
                return new Response(RequestType.Port,
                    await GetStringResponseAsync(buffer, bytesRead));
            case RequestType.Disks:
                return new Response(RequestType.Disks,
                    await GetStringResponseAsync(buffer, bytesRead));
            default:
                throw new ApplicationException("Invalid signature!");
        }
    }

    private async Task<string> SaveFileAsync(byte[] buffer, int bytesRead, string request)
    {
        byte[] responseBuffer = new byte[8 * 1024];
        Array.Copy(buffer, 1, responseBuffer, 0, buffer.Length - 1);
        string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{Path.GetFileName(request)}";
        FileStream fileStream =
            new FileStream(path, FileMode.Create);

        await fileStream.WriteAsync(responseBuffer, 0, bytesRead - 1);
        await fileStream.FlushAsync();

        while (bytesRead != 0)
        {
            if (_networkStream.DataAvailable == false)
                break;
            bytesRead = await _networkStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
            
            await fileStream.WriteAsync(responseBuffer, 0, bytesRead);
            await fileStream.FlushAsync();
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
        await _networkStream.WriteAsync(new[] { (byte) RequestType.Disconnect }, 0, 1);
        await _networkStream.FlushAsync();
        CloseSocket();
    }

    private void CloseSocket()
    {
       _clientSocket.Close();
       Disconnected?.Invoke();
    }

    public Action Disconnected { get; set; }
}