namespace TCPClientApp.Domain;

public struct Response
{
    public ResponseType Type { get; }
    
    public string[] Contents { get; }

    public Response(ResponseType type, string[] contents)
    {
        Type = type;
        Contents = contents;
    }
}