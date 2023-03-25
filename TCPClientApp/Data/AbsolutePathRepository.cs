namespace TCPClientApp.Data;

public class AbsolutePathRepository
{
    private string _path { get; set; }

    public void Concat(string fileName) => _path += fileName;

    public void Clear() => _path = "";
}