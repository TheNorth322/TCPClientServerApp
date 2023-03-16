using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace TCPClientApp.Model;

public class TCPServer : IServer
{
    private Socket _listener;

    public async Task Connect(string ip, string port)
    {
        IPEndPoint ipEndPoint = IPEndPoint.Parse($"{ip}:{port}");
        using Socket listener = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _listener = listener;

        _listener.Bind(ipEndPoint);
        _listener.Listen(100);
        AcceptRequest();
    }

    private async Task AcceptRequest()
    {
        Socket handler = await _listener.AcceptAsync();
        while (true)
        {
            byte[] buffer = new byte[1024];
            int received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            string response = Encoding.UTF8.GetString(buffer, 0, received);

            if (response.IndexOf("<|EOM|>") > -1)
            {
                string ackMessage = ParseRequest(response).Invoke(response);
                byte[] messageBytes = Encoding.UTF8.GetBytes(ackMessage);
                
                await handler.SendAsync(messageBytes, 0);

                break;
            }
        }
    }

    private Func<string, string> ParseRequest(string request)
    {
        FileAttributes attributes = File.GetAttributes(request);

        if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            return GetDirectoryFiles;
        else
            return GetTextFileContents;
    }

    private string GetDirectoryFiles(string path)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        FileInfo[] files = directoryInfo.GetFiles();
        DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
        StringBuilder stringBuilder = new StringBuilder();

        foreach (var file in files)
            stringBuilder.Append($"{file.Name}, ");

        foreach (var directory in subDirectories)
            stringBuilder.Append($"{directory.Name}, ");

        return stringBuilder.ToString();
    }

    private string GetTextFileContents(string path)
    {
        return File.ReadAllText(path);
    }
}