using System.Net.Sockets;
using System.Text;
using System;

namespace TCPClientApp.Model;

public class ClientHandler : IDisposable
{
    private RequestAnalyzer _requestAnalyzer;
    private TcpClient _clientSocket;
    private ILogger _logger;

    public ClientHandler()
    {
        _requestAnalyzer = new RequestAnalyzer();
    }

    public ClientHandler(TcpClient client, ILogger logger) : this()
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
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
                byte[] buffer = new byte[1024];
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

    private string ParseRequest(string request) => _requestAnalyzer.Analyze(request);

    private void CheckConneciton()
    {
        if (_clientSocket.Connected == false)
            Disconnect();
    }

    private void Disconnect()
    {
        _clientSocket.Close();
        _logger.Log(" >> Client disconnected");
    }

    public void Dispose()
    {
        Disconnect();
        _clientSocket.Dispose();
    }
}