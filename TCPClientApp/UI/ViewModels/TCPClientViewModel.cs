﻿using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
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
    private string _absolutePath;
    private ListBoxItemViewModel _selectedListBoxItem;

    public ListBoxItemViewModel SelectedListBoxItem
    {
        get { return _selectedListBoxItem; }
        set
        {
            _selectedListBoxItem = value;
            UpdateRequest();
            OnPropertyChange(nameof(SelectedListBoxItem));
        }
    }

    public TCPClientViewModel()
    {
        EndPoint = "127.0.0.1:8888";
        socket = new TCPClient();
        parser = new ResponseParser();
    }

    private void UpdateRequest()
    {
        Request = (_absolutePath.EndsWith(@"\") || _absolutePath == "")
            ? _absolutePath + SelectedListBoxItem.Header
            : _absolutePath + @$"\{SelectedListBoxItem.Header}";
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
                _canExecute => !socket.Connected()
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
                _canExecute => socket.Connected()
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
                _canExecute => socket.Connected()
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
                _canExecute => socket.Connected()
            );
        }
    }

    private RelayCommand _goBackCommand;

    public RelayCommand GoBackCommand
    {
        get
        {
            return _goBackCommand ?? new RelayCommand(
                _execute => GoBack(),
                _canExecute => socket.Connected()
            );
        }
    }

    private async void SendRequest()
    {
        try
        {
            ClientLog += $"Client sent: {Request}\n";
            string response = await socket.SendRequestAsync(Request + "<|EOM|>");
            Response parsedResponse = parser.Parse(response);

            if (parsedResponse.Type == ResponseType.DirectoryContents)
            {
                ClientLog += $"Client received: {response}\n";
                UpdateDirectoryContents(parsedResponse.Contents);
                UpdateAbsolutePath();
            }
            else if (parsedResponse.Type == ResponseType.FileContents)
                ClientLog += $"Client received: {parsedResponse.Contents[0]}\n";
            else
                ClientLog += $"Client received: {parsedResponse.Contents[0]}\n";
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ClearDirectoryContents()
    {
        DirectoryContents = new ObservableCollection<ListBoxItemViewModel>();
    }

    private void Disconnect()
    {
        try
        {
            socket.Disconnect();
            ClientLog += $"Client disconnected from: {EndPoint}\n";
            ClearDirectoryContents();
            Request = "";
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
            ClientLog += $"Client connected to: {EndPoint}\n";
            GetDisks();
        }
        catch (Exception ex)
        {
            MessageBox_Show(null, ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GoBack()
    {
        Request = Request.Replace(@$"\{SelectedListBoxItem.Header}", "");
        _absolutePath = Request;
    }

    private void GetDisks()
    {
        Request = @"\";
        SendRequest();
        Request = "";
    }

    private void UpdateAbsolutePath()
    {
        _absolutePath = Request;
    }

    private void UpdateDirectoryContents(string[] contents)
    {
        ObservableCollection<ListBoxItemViewModel> directories = new ObservableCollection<ListBoxItemViewModel>();
        foreach (string line in contents)
            directories.Add(new ListBoxItemViewModel(line));
        DirectoryContents = directories;
    }
}