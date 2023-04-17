namespace TCPClientApp.Model;

public interface IRequestAnalyzer
{
    string Analyze(string request);
}