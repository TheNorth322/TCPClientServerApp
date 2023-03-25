using System.Text;

namespace TCPClientApp.Model;

public class RequestAnalyzer
{
    public string GetLogicalDrives(string _)
    {
        string[] drives = Directory.GetLogicalDrives();
        StringBuilder sr = new StringBuilder();

        foreach (string drive in drives)
            sr.Append($"{drive}|");
        
        return sr.ToString();
    }

    public string GetDirectoryFiles(string path)
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

    public string GetTextFileContents(string path)
    {
        return File.ReadAllText(path);
    }
}