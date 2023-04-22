using System;
using System.IO;
using System.Linq;
using System.Text;

namespace TCPClientApp.Domain;

public class ResponseParser
{
    public Response Parse(string response, string request)
    {
        string[] parsedResponse = response.Split("|");

        return parsedResponse.First() switch
        {
            "type=dirContents" => new Response(ResponseType.DirectoryContents, Join(RemoveType(parsedResponse))),
            "type=fileName" => new Response(ResponseType.FileName, Join(RemoveType(parsedResponse))),
            "type=system" => new Response(ResponseType.System, Join(RemoveType(parsedResponse))),
            "type=port" => new Response(ResponseType.Port, Join(RemoveType(parsedResponse))),
            "type=fileContents" => new Response(ResponseType.FileContents, SaveInFile(parsedResponse, request)),
            _ => throw new ApplicationException("Wrong request")
        };
    }
    
    private string[] RemoveType(string[] parsedResponse)
    {
        string[] response = new string[parsedResponse.Length - 1];

        for (int i = 1; i < parsedResponse.Length; i++)
            response[i - 1] = parsedResponse[i];

        return response;
    }

    private string Join(string[] parsedResponse)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (string line in parsedResponse)
            stringBuilder.Append($"{line}|");
        stringBuilder.Remove(stringBuilder.Length - 1, 1);
        return  stringBuilder.ToString();
    }

    private string SaveInFile(string[] parsedResponse, string request)
    {
        string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{Path.GetFileName(request)}";
        FileStream fs = File.Open(path, FileMode.OpenOrCreate);
        fs.Write(Encoding.UTF8.GetBytes(Join(RemoveType(parsedResponse))));
        return $"File save in {path}";
    }
}