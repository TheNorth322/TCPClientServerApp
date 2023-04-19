﻿using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPClientApp.Model;

public class TCPServer
{
    private ILogger _logger;
    private Port[] _ports;
    private int startPort = 8888;

    public TCPServer(ILogger logger)
    {
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));
        _ports = new Port[1000];
        _logger = logger;
        InitializePorts();
    }

    public async Task Start()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, startPort);
        listener.Start();
        _logger.Log(" >> Server started!");
        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _logger.Log(" >> Client connected to localhost:8888");
            Port port = FindFreePort();
            _logger.Log($" >> Found new port: {port.PortValue}");
            await SendNewPort(client, port);
            _logger.Log($" >> Waiting for connection");
            TcpClient cl = await WaitForConnection(port);
            _logger.Log($" >> Client connected to localhost:{port.PortValue}");
            port.Occupied = true;
            ClientHandler clientHandler = new ClientHandler(cl, _logger, port);
            clientHandler.Start();
        }

        listener.Stop();
    }

    private async Task SendNewPort(TcpClient client, Port port)
    {
        NetworkStream networkStream = client.GetStream();
        byte[] responseBytes = Encoding.UTF8.GetBytes($"type=port|{port.PortValue}");

        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        await networkStream.FlushAsync();

        Disconnect(client);
    }

    private async Task<TcpClient> WaitForConnection(Port port)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, port.PortValue);
        listener.Start();
        TcpClient client = await listener.AcceptTcpClientAsync();
        listener.Stop();
        _logger.Log($" >> Listener on port {port.PortValue} started!");
        return client;
    }

    private Port FindFreePort()
    {
        foreach (Port port in _ports)
        {
            if (port.Occupied == false)
                return port;
        }

        throw new ApplicationException("There is no free port.");
    }

    private void InitializePorts()
    {
        for (int i = 0; i < 1000; i++)
            _ports[i] = new Port(8889 + i, false);
    }

    private void Disconnect(TcpClient client)
    {
        client.Close();
        client.Dispose();
    }
}