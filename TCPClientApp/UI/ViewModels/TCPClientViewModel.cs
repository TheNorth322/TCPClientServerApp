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
    private bool _connected;
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
        _serverDirectoryContents = new ObservableCollection<ListBoxItemViewModel>();
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

    private void UpdateRequest()
    {
        if (_absolutePath == @"\") _absolutePath = "";
        Request = (_absolutePath.EndsWith(@"\") || _absolutePath == "")
            ? _absolutePath + SelectedServerListBoxItem.Header
            : _absolutePath + @$"\{SelectedServerListBoxItem.Header}";
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
                _execute => SendRequest(),
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

    private async Task SendRequest()
    {
        try
        {
            ClientLog += $"Client sent: {Request}\n";
            Response response = await socket.SendRequestAsync(Request);

            switch (response.Type)
            {
                case ResponseType.DirectoryContents:
                    ClientLog += $"Client received: {response.Contents}\n";
                    UpdateServerDirectoryContents(response.Contents.Split('|'));
                    UpdateAbsolutePath();
                    break;
                case ResponseType.FileContents:
                    ClientLog += $"Client received: {response.Contents}\n";
                    break;
                case ResponseType.System:
                    ClientLog += $"Client received: {response.Contents}\n";
                    socket.Disconnect();
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
            Request = "200";
            await SendRequest();
            Request = "";
            _connected = false;
            ClientLog += $"Client disconnected from: {EndPoint}\n";
            ClearDirectoryContents();
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Connect()
    {
        try
        {
            await socket.ConnectAsync(EndPoint);
            _connected = true;
            ClientLog += $"Client connected to: {EndPoint}\n";
            GetDisks();
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void GetDisks()
    {
        Request = @"\";
        await SendRequest();
        Request = "";
    }

    private void UpdateAbsolutePath()
    {
        _absolutePath = Request;
    }

    private void UpdateServerDirectoryContents(string[] contents)
    {
        ObservableCollection<ListBoxItemViewModel> directories = new ObservableCollection<ListBoxItemViewModel>();
        foreach (string line in contents)
            directories.Add(new ListBoxItemViewModel(line));
        ServerDirectoryContents = directories;
    }
}