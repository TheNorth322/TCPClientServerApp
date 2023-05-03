using System.Net.Sockets;
using System.Text;
using System;
using TCPClientApp.Domain;

namespace TCPClientApp.Model;

public class ClientHandler : IDisposable
{
    private TcpClient _clientSocket;
    private ILogger _logger;
    private Port port;
    private NetworkStream _networkStream;

    public ClientHandler(TcpClient client, ILogger logger, Port _port)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        port = _port;
        _clientSocket = client;
        _networkStream = _clientSocket.GetStream();
        _logger = logger;
    }

    public void Start()
    {
        Thread thread = new Thread(Listen);
        thread.Start();
    }

    private async void Listen()
    {
        try
        {
            while (true)
            {
                byte[] buffer = new byte[1024 * 8];
                StringBuilder request = new StringBuilder();

                await GetRequest(buffer, request);
                await SendResponse(request.ToString());

                _logger.Log($"Socket server received message: \"{request}\"");
            }
        }
        catch (Exception ex)
        {
            Disconnect();
        }
    }

    private async Task GetRequest(byte[] buffer, StringBuilder request)
    {
        CheckConneciton();
        int bytesRead = await _networkStream.ReadAsync(buffer);

        while (bytesRead > 0)
        {
            request.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

            if (_networkStream.DataAvailable == false)
                break;

            bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
        }
    }

    private async Task SendResponse(string request)
    {
        byte[] type = Encoding.UTF8.GetBytes(request.Substring(0, 1));
        request = request.Substring(1, request.Length - 1);
        try
        {
            switch ((RequestType) type[0])
            {
                case RequestType.DirectoryContents:
                    await SendStringAsync(GetDirectoryContents(request), RequestType.DirectoryContents);
                    break;
                case RequestType.FileContents:
                    await SendFileContentsAsync(request);
                    break;
                case RequestType.Ping:
                    break;
                case RequestType.Disks:
                    await SendStringAsync(GetLogicalDrives(request), RequestType.Disks);
                    break;
                case RequestType.Disconnect:
                    Disconnect();
                    break;
                default:
                    throw new ApplicationException("Wrong signature!");
            }
        }
        catch (Exception ex)
        {
            await SendStringAsync(ex.Message, RequestType.Exception);
        }
    }

    private async Task SendFileContentsAsync(string request)
    {
        FileStream fileStream = new FileStream(request, FileMode.Open);
        byte[] buffer = new byte[1024 * 8];
        
        buffer[0] = (byte) RequestType.FileContents;
        int bytesRead = await fileStream.ReadAsync(buffer, 1, buffer.Length - 1);

        await _networkStream.WriteAsync(buffer, 0, bytesRead + 1);
        await _networkStream.FlushAsync();

        while (bytesRead != 0)
        {
            bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
            await _networkStream.WriteAsync(buffer, 0, bytesRead);
            await _networkStream.FlushAsync();
        }

        fileStream.Close();
    }

    private async Task SendStringAsync(string response, RequestType type)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(response);
        byte[] responseBytes = new byte[bytes.Length + 1];
        _logger.Log($"Server sent: {response}");
        responseBytes[0] = (byte) type;
        Array.Copy(bytes, 0, responseBytes, 1, bytes.Length);

        await _networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        await _networkStream.FlushAsync();
    }

    private void CheckConneciton()
    {
        if (_clientSocket.Connected == false)
            Disconnect();
    }

    private void Disconnect()
    {
        port.Occupied = false;
        _clientSocket.Close();
        _clientSocket.Dispose();
        _logger.Log(" >> Client disconnected");
    }


    private string GetLogicalDrives(string _)
    {
        string[] drives = Directory.GetLogicalDrives();
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < drives.Length; i++)
            sb.Append((i == drives.Length - 1)
                ? $"{drives[i]}"
                : $"{drives[i]}|"
            );

        return sb.ToString();
    }

    private string GetDirectoryContents(string path)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        FileInfo[] files = directoryInfo.GetFiles();
        DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
        StringBuilder stringBuilder = new StringBuilder();

        foreach (var file in files)
            stringBuilder.Append($"{file.Name}|");

        for (int i = 0; i < subDirectories.Length; i++)
        {
            stringBuilder.Append((i == subDirectories.Length - 1)
                ? $"{subDirectories[i].Name}"
                : $"{subDirectories[i].Name}|"
            );
        }


        return stringBuilder.ToString();
    }

    public void Dispose()
    {
        Disconnect();
    }
}