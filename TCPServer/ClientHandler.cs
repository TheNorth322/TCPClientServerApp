using System.Net.Sockets;
using System.Text;
using System;

namespace TCPClientApp.Model;

public class ClientHandler : IDisposable
{
    private TcpClient _clientSocket;
    private ILogger _logger;
    private Port port;
    private string directoryRequestEnding = "type=dirContents";
    private string fileRequestEnding = "type=fileContents";
    private string fileNameRequestEnding = "type=fileName";
    private string exceptionRequestEnding = "type=exception";
    private string systemRequestEnding = "type=system";

    public ClientHandler(TcpClient client, ILogger logger, Port _port)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        port = _port;
        _clientSocket = client;
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
                byte[] buffer = new byte[1024 * 32];
                NetworkStream networkStream = _clientSocket.GetStream();
                StringBuilder request = new StringBuilder();

                await GetRequest(networkStream, buffer, request);
                await SendResponse(networkStream, request.ToString());

                _logger.Log($"Socket server received message: \"{request}\"");
            }
        }
        catch (Exception ex)
        {
            Disconnect();
        }
    }

    private async Task GetRequest(NetworkStream networkStream, byte[] buffer, StringBuilder request)
    {
        CheckConneciton();
        int bytesRead = await networkStream.ReadAsync(buffer);
        while (bytesRead > 0)
        {
            request.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

            if (networkStream.DataAvailable == false)
                break;

            bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
        }
    }

    private async Task SendResponse(NetworkStream networkStream, string request)
    {
        string ackMessage = ParseRequest(request);
        byte[] responseBytes = Encoding.UTF8.GetBytes(ackMessage);

        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        await networkStream.FlushAsync();

        if (request == "200")
            Disconnect();
    }

    private string ParseRequest(string request)
    {
        try
        {
            switch (request)
            {
                case "200":
                    return $"{systemRequestEnding}|Disconnected";
                case @"\":
                    return $"{directoryRequestEnding}|{GetLogicalDrives(request)}";
                default:
                {
                    FileAttributes attributes = File.GetAttributes(request);
                    if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                        return $"{directoryRequestEnding}|{GetDirectoryFiles(request)}";
                    else if (Path.GetExtension(request) != ".txt")
                        return $"{fileNameRequestEnding}|{Path.GetFileName(request)}";
                    break;
                }
            }

            return $"{fileRequestEnding}|{GetTextFileContents(request)}";
        }
        catch (Exception ex)
        {
            return $"{exceptionRequestEnding}|{ex.Message}";
        }
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

    public void Dispose()
    {
        Disconnect();
    }

    private string GetLogicalDrives(string _)
    {
        string[] drives = Directory.GetLogicalDrives();
        StringBuilder sr = new StringBuilder();

        for (int i = 0; i < drives.Length; i++)
            sr.Append((i == drives.Length - 1) 
                ? $"{drives[i]}" 
                : $"{drives[i]}|"
            );

        return sr.ToString();
    }

    private string GetDirectoryFiles(string path)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        FileInfo[] files = directoryInfo.GetFiles();
        DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
        StringBuilder stringBuilder = new StringBuilder();

        foreach (var file in files)
            stringBuilder.Append($"{file.Name}|");

        foreach (var directory in subDirectories)
            stringBuilder.Append($"{directory.Name}|");

        return stringBuilder.ToString();
    }

    private string GetTextFileContents(string path)
    {
        return File.ReadAllText(path);
    }
}