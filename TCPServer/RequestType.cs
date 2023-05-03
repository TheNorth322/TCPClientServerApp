namespace TCPClientApp.Domain;

public enum RequestType : byte
{
    DirectoryContents = 1,
    FileContents = 2,
    Ping = 3,
    Port = 6,
    Disks = 7,
    Exception = 4,
    Disconnect = 8
}