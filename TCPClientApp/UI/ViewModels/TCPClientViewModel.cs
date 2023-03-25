using System.Collections.ObjectModel;
using TCPClientApp.Domain;
using TCPClientApp.Model;

namespace TCPClientApp.UI.ViewModels;

public class TCPClientViewModel : ViewModelBase
{
    private string[] _directories;

    private ObservableCollection<ListBoxItemViewModel> _directoryContents;

    private string _endPoint;
    private string _request;
    private TCPClient socket;
    private ResponseParser parser;
    private string clientLog;
    
    public TCPClientViewModel()
    {
        socket = new TCPClient();
        parser = new ResponseParser();
    }

    public string ClientLog
    {
        get
        {
            return clientLog;
        }
        set
        {
            clientLog = value;
            OnPropertyChange(nameof(clientLog));
        }
    }
    public string Request
    {
        get { return _request; }
        set
        {
            _request = value;
            OnPropertyChange(nameof(Request));
        }
    }

    public ObservableCollection<ListBoxItemViewModel> DirectoryContents
    {
        get { return _directoryContents; }
        set
        {
            _directoryContents = value;
            OnPropertyChange(nameof(DirectoryContents));
        }
    }

    public string EndPoint
    {
        get { return _endPoint; }
        set
        {
            _endPoint = value;
            OnPropertyChange(nameof(EndPoint));
        }
    }

    private RelayCommand _connect;

    public RelayCommand ConnectCommand
    {
        get
        {
            return _connect ?? new RelayCommand(
                _execute => Connect(),
                _canExecute => true 
            );
        }
    }

    private RelayCommand _disconnect;
    public RelayCommand DisconnectCommand
    {
        get
        {
            return _disconnect ?? new RelayCommand(
                _execute => Disconnect(),
                _canExecute => true 
            );
        }
    }

    private RelayCommand _sendRequest;
    public RelayCommand SendRequestCommand
    {
        get
        {
            return _sendRequest ?? new RelayCommand(
                _execute => SendRequest(),
                _canExecute => true 
            );
        }
    }

    private void Disconnect() => socket.Disconnect();        

    private void Connect() => socket.ConnectAsync(EndPoint);

    private void SendRequest()
    {
        string response = socket.SendRequestAsync(Request).Result;
        Response parsedResponse = parser.Parse(response);
        
        if (parsedResponse.Type == ResponseType.DirectoryContents)
            UpdateDirectoryContents(parsedResponse.Contents);
        else if (parsedResponse.Type == ResponseType.FileContents)
            ClientLog += parsedResponse.Contents[0];
    }

    private void UpdateDirectoryContents(string[] contents)
    {
        ObservableCollection<ListBoxItemViewModel> directories = new ObservableCollection<ListBoxItemViewModel>();
        foreach (string line in contents)
           directories.Add(new ListBoxItemViewModel(line));
        DirectoryContents = directories;
    }
}