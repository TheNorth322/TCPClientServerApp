using System.Linq;
using System.Text;

namespace TCPClientApp.Domain;

public class ResponseParser
{
    public Response Parse(string response)
    {
        response = response.Replace("<|EOM|>", "");
        string[] parsedResponse = response.Split("|");
        
        if (parsedResponse.Last() == "type=dirContents")
            return new Response(ResponseType.DirectoryContents, RemoveType(parsedResponse));
        else if (parsedResponse.Last() == "type=fileName")
            return new Response(ResponseType.FileName, RemoveType(parsedResponse));
        else
            return new Response(ResponseType.FileContents, Join(RemoveType(parsedResponse)));
    }

    private string[] RemoveType(string[] parsedResponse)
    {
        string[] response = new string[parsedResponse.Length - 1];

        for (int i = 0; i < parsedResponse.Length - 1; i++)
            response[i] = parsedResponse[i];

        return response;
    }

    private string[] Join(string[] parsedResponse)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (string line in parsedResponse)
            stringBuilder.Append($"{line}|");
        stringBuilder.Remove(stringBuilder.Length - 1, 1);
        return new string[] { stringBuilder.ToString() };
    }
}