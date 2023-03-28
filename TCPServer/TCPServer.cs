using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPClientApp.Model;

public class TCPServer
{
    private string directoryRequestEnding = "type=dirContents";
    private string fileRequestEnding = "type=fileContents";
    private string fileNameRequestEnding = "type=fileName";
    private RequestAnalyzer _requestAnalyzer;
    public TCPServer()
    {
        _requestAnalyzer = new RequestAnalyzer();
    }

    public async Task Connect()
    {
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(ipPoint);
        socket.Listen(100);
        using Socket client = await socket.AcceptAsync();
        Console.WriteLine($"{client.RemoteEndPoint} connected.");
        while (true)
            await AcceptRequest(client);
    }

    private async Task AcceptRequest(Socket client)
    {
        string eom = "<|EOM|>";
        while (true)
        {
            byte[] buffer = new byte[1_024];
            int received = await client.ReceiveAsync(buffer, SocketFlags.None);
            string request = Encoding.UTF8.GetString(buffer, 0, received);

            if (request.IndexOf(eom) > -1)
            {
                Console.WriteLine(
                    $"Socket server received message: \"{request}\"");
                request = request.Replace(eom, "");
                string ackMessage = ParseRequest(request) + eom;
                byte[] echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                await client.SendAsync(echoBytes, 0);
                Console.WriteLine(
                    $"Socket server sent acknowledgment: \"{ackMessage}\"");
                break;
            }
        }
    }

    private string ParseRequest(string request)
    {
        FileAttributes attributes = File.GetAttributes(request);
        Console.WriteLine($"asd{request}");
        if (request == @"\")
            return _requestAnalyzer.GetLogicalDrives(request) + directoryRequestEnding;
        else if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            return _requestAnalyzer.GetDirectoryFiles(request) + directoryRequestEnding;
        else if (Path.GetExtension(request) != ".txt")
            return Path.GetFileName(request) + $"|{fileNameRequestEnding}";
        else;
            return _requestAnalyzer.GetTextFileContents(request) + $"|{fileRequestEnding}";
    }
}