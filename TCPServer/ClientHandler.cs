using System.Net.Sockets;
using System.Text;
using System;

namespace TCPClientApp.Model;

public class ClientHandler : IDisposable
{
    private string directoryRequestEnding = "type=dirContents";
    private string fileRequestEnding = "type=fileContents";
    private string fileNameRequestEnding = "type=fileName";
    private string exceptionRequestEnding = "type=exception";

    private RequestAnalyzer _requestAnalyzer;
    private TcpClient _clientSocket;

    public ClientHandler()
    {
        _requestAnalyzer = new RequestAnalyzer();
    }

    public ClientHandler(TcpClient client) : this()
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        _clientSocket = client;
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
                byte[] buffer = new byte[1024];
                NetworkStream networkStream = _clientSocket.GetStream();
                StringBuilder request = new StringBuilder();

                await GetRequest(networkStream, buffer, request);
                await SendResponse(networkStream, request.ToString());
                
                Console.WriteLine(
                    $"Socket server received message: \"{request}\"");
            }
        }
        catch (Exception ex)
        {
            Disconnect();
        }
    }

    private async Task GetRequest(NetworkStream networkStream, byte[] buffer, StringBuilder request)
    {
        int bytesRead = await networkStream.ReadAsync(buffer);
        while (bytesRead > 0)
        {
            request.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

            if (networkStream.DataAvailable == false)
                break;

            bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
        }

        if (request.ToString() == "")
            Disconnect();
    }

    private async Task SendResponse(NetworkStream networkStream, string request)
    {
        string ackMessage = ParseRequest(request);
        byte[] responseBytes = Encoding.UTF8.GetBytes(ackMessage);

        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        await networkStream.FlushAsync();

        Console.WriteLine(
            $"Socket server sent acknowledgment: \"{ackMessage}\"");
    }

    private string ParseRequest(string request)
    {
        try
        {
            FileAttributes attributes = File.GetAttributes(request);
            if (request == @"\")
                return _requestAnalyzer.GetLogicalDrives(request) + directoryRequestEnding;
            else if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                return _requestAnalyzer.GetDirectoryFiles(request) + directoryRequestEnding;
            else if (Path.GetExtension(request) != ".txt")
                return Path.GetFileName(request) + $"|{fileNameRequestEnding}";
            return _requestAnalyzer.GetTextFileContents(request) + $"|{fileRequestEnding}";
        }
        catch (Exception ex)
        {
            return ex.Message + $"|{exceptionRequestEnding}";
        }
    }

    private void Disconnect()
    {
        _clientSocket.Close();
    }

    public void Dispose()
    {
        Disconnect();
        _clientSocket.Dispose();
    }
}