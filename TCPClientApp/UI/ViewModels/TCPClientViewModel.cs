using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TCPClientApp.Domain;
using TCPClientApp.Model;

namespace TCPClientApp.UI.ViewModels;

public class TCPClientViewModel : ViewModelBase
{
    private string[] _directories;
    private ObservableCollection<ListBoxItemViewModel> _serverDirectoryContents;
    private ListBoxItemViewModel? _selectedServerListBoxItem;
    private RequestParser _requestParser;
    private bool _connected;
    private string _enterEndPoint;
    private string _endPoint;
    private string _request;
    private TCPClient socket;
    private string clientLog;
    private string _absolutePath;
    private string _path;


    public TCPClientViewModel()
    {
        EndPoint = "127.0.0.1:8888";
        socket = new TCPClient();
        socket.Disconnected += OnDisconnect;
        _serverDirectoryContents = new ObservableCollection<ListBoxItemViewModel>();
        _requestParser = new RequestParser();
    }

    public ListBoxItemViewModel SelectedServerListBoxItem
    {
        get { return _selectedServerListBoxItem; }
        set
        {
            _selectedServerListBoxItem = value;
            OnPropertyChange(nameof(SelectedServerListBoxItem));
            UpdateRequest();
        }
    }

    public string ClientLog
    {
        get { return clientLog; }
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

    public ObservableCollection<ListBoxItemViewModel> ServerDirectoryContents
    {
        get { return _serverDirectoryContents; }
        set
        {
            _serverDirectoryContents = value;
            OnPropertyChange(nameof(ServerDirectoryContents));
        }
    }

    public string EndPoint
    {
        get { return _enterEndPoint; }
        set
        {
            _enterEndPoint = value;
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
                _canExecute => !_connected
            );
        }
    }

    private RelayCommand _clearLog;

    public RelayCommand ClearLogCommand
    {
        get
        {
            return _clearLog ?? new RelayCommand(
                _execute => ClearLog(),
                _canExecute => true
            );
        }
    }

    private void ClearLog()
    {
        ClientLog = "";
    }

    private RelayCommand _disconnect;

    public RelayCommand DisconnectCommand
    {
        get
        {
            return _disconnect ?? new RelayCommand(
                _execute => Disconnect(),
                _canExecute => _connected
            );
        }
    }

    private RelayCommand _sendRequest;

    public RelayCommand SendRequestCommand
    {
        get
        {
            return _sendRequest ?? new RelayCommand(
                _execute => SendRequest(Request),
                _canExecute => _connected
            );
        }
    }

    private RelayCommand _getDisksCommand;

    public RelayCommand GetDisksCommand
    {
        get
        {
            return _getDisksCommand ?? new RelayCommand(
                _execute => GetDisks(),
                _canExecute => _connected
            );
        }
    }

    private async Task SendRequest(string request)
    {
        try
        {
            ClientLog += $"Client sent: {request}\n";
            request = _requestParser.Parse(request);
            Response response = await socket.SendRequestAsync(request);

            switch (response.Type)
            {
                case RequestType.DirectoryContents:
                    ClientLog += $"Client received: {response.Contents}\n";
                    UpdateServerDirectoryContents(response.Contents.Split('|'));
                    UpdateAbsolutePath();
                    break;
                case RequestType.Disks:
                    ClientLog += $"Client received: {response.Contents}\n";
                    UpdateServerDirectoryContents(response.Contents.Split('|'));
                    break;
                case RequestType.FileContents:
                    ClientLog += $"Client received: {response.Contents}\n";
                    break;
                default:
                    ClientLog += $"Client received: {response.Contents}\n";
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ClearDirectoryContents()
    {
        ServerDirectoryContents = new ObservableCollection<ListBoxItemViewModel>();
    }

    private async Task Disconnect()
    {
        try
        {
            await socket.DisconnectAsync();
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnDisconnect()
    {
        _connected = false;
        ClientLog += $"Client disconnected from: {_endPoint}\n";
        ClearDirectoryContents();
    }

    private async void Connect()
    {
        try
        {
            await socket.ConnectAsync(EndPoint);
            _endPoint = socket.IpEndPoint.ToString();
            _connected = true;
            ClientLog += $"Client connected to: {_endPoint}\n";
            GetDisks();
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void GetDisks()
    {
        Request = "";
        _absolutePath = "";
        await SendRequest(@"\");
    }

    private void UpdateAbsolutePath()
    {
        _absolutePath = Request;
    }

    private void UpdateRequest()
    {
        if (Request == @"\") Request = "";
        Request = (_absolutePath.EndsWith(@"\") || _absolutePath == "")
            ? _absolutePath + SelectedServerListBoxItem.Header
            : _absolutePath + $"\\{SelectedServerListBoxItem.Header}";
    }

    private void UpdateServerDirectoryContents(string[] contents)
    {
        ObservableCollection<ListBoxItemViewModel> directories = new ObservableCollection<ListBoxItemViewModel>();
        foreach (string line in contents)
            directories.Add(new ListBoxItemViewModel(line));
        ServerDirectoryContents = directories;
    }
}