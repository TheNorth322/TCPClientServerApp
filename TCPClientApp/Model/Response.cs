namespace TCPClientApp.Domain;

public struct Response
{
    public RequestType Type { get; }
    
    public string Contents { get; }

    public Response(RequestType type, string contents)
    {
        Type = type;
        Contents = contents;
    }
}