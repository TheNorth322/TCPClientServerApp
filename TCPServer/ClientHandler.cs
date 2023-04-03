using System.Net.Sockets;
using System.Text;

namespace TCPClientApp.Model;

public class ClientHandler
{
    private string directoryRequestEnding = "type=dirContents";
    private string fileRequestEnding = "type=fileContents";
    private string fileNameRequestEnding = "type=fileName";
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
        while (true)
        {
            byte[] buffer = new byte[1024];
            NetworkStream networkStream = _clientSocket.GetStream();
            int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            StringBuilder request = new StringBuilder();

            while (bytesRead > 0)
            {
                request.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                
                if (networkStream.DataAvailable == false) 
                    break;
                
                bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            }

            Console.WriteLine(
                $"Socket server received message: \"{request}\"");

            string ackMessage = ParseRequest(request.ToString());
            byte[] responseBytes = Encoding.UTF8.GetBytes(ackMessage);

            await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
            await networkStream.FlushAsync();

            Console.WriteLine(
                $"Socket server sent acknowledgment: \"{ackMessage}\"");
            break;
        }
    }

    private string ParseRequest(string request)
    {
        FileAttributes attributes = File.GetAttributes(request);
        if (request == @"\")
            return _requestAnalyzer.GetLogicalDrives(request) + directoryRequestEnding;
        else if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            return _requestAnalyzer.GetDirectoryFiles(request) + directoryRequestEnding;
        else if (Path.GetExtension(request) != ".txt")
            return Path.GetFileName(request) + $"|{fileNameRequestEnding}";
        else ;
        return _requestAnalyzer.GetTextFileContents(request) + $"|{fileRequestEnding}";
    }
}