using System.Threading.Tasks;

namespace TCPClientApp.Model;

public interface IClient
{
    Task ConnectAsync(string ip, string port);
    Task<string> SendRequestAsync(string message);
    public void Disconnect();
}