using System.Threading.Tasks;

namespace TCPClientApp.Model;

public interface IServer
{
    Task Connect(string ip, string port);
}