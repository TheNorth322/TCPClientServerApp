using System.Text;

namespace TCPClientApp.Model;

public class RequestAnalyzer : IRequestAnalyzer
{
    private string directoryRequestEnding = "type=dirContents";
    private string fileRequestEnding = "type=fileContents";
    private string fileNameRequestEnding = "type=fileName";
    private string exceptionRequestEnding = "type=exception";
    private string systemRequestEnding = "type=system";

    public string Analyze(string request)
    {
        try
        {
            switch (request)
            {
                case "200":
                    return $"{systemRequestEnding}|Disconnected|";
                case @"\":
                    return $"{directoryRequestEnding}|{GetLogicalDrives(request)}";
                default:
                {
                    FileAttributes attributes = File.GetAttributes(request);
                    if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                        return $"{directoryRequestEnding}|{GetDirectoryFiles(request)}";
                    else if (Path.GetExtension(request) != ".txt")
                        return $"{fileNameRequestEnding}|{Path.GetFileName(request)}";
                    break;
                }
            }

            return $"{fileRequestEnding}|{GetTextFileContents(request)}";
        }
        catch (Exception ex)
        {
            return ex.Message + $"|{exceptionRequestEnding}";
        }
    }

    private string GetLogicalDrives(string _)
    {
        string[] drives = Directory.GetLogicalDrives();
        StringBuilder sr = new StringBuilder();

        foreach (string drive in drives)
            sr.Append($"{drive}|");

        return sr.ToString();
    }

    private string GetDirectoryFiles(string path)
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

    private string GetTextFileContents(string path)
    {
        return File.ReadAllText(path);
    }
}