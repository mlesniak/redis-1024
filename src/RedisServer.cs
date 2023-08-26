using System.ComponentModel.Design;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lesniak.Redis;

public class RedisServer
{
    private readonly TcpListener _server;
    private readonly CommandHandler _commandHandler;

    public RedisServer(int port = 6379)
    {
        _server = new TcpListener(IPAddress.Loopback, port);
        _commandHandler = new CommandHandler(new Memory());
    }

    public void Start()
    {
        _server.Start();
        Console.WriteLine("Server started, listening for clients.");
        Listen();
    }

    private void Listen()
    {
        TcpClient client = _server.AcceptTcpClient();
        Console.WriteLine("Client connected");
        var stream = client.GetStream();
        HandleConnection(stream);
    }

    private void HandleConnection(NetworkStream stream)
    {
        while (true)
        {
            var commandline = ReadCommandline(stream);
            var responseBytes = _commandHandler.Execute(commandline);
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        // TODO(mlesniak) We never close this connection 🙈 ...
    }

    // Command is always an array 
    private static RedisData ReadCommandline(NetworkStream stream)
    {
        // TODO(mlesniak) We are not handling larger input.
        byte[] buffer = new byte[16384];
        stream.Read(buffer);
        return RedisDataParser.Parse(buffer);
    }
}