using System.Net;
using System.Net.Sockets;

using Lesniak.Redis.Core;
using Lesniak.Redis.Model;
using Lesniak.Redis.Storage;

namespace Lesniak.Redis.Server;

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
        while (true)
        {
            TcpClient client = _server.AcceptTcpClient();
            Console.WriteLine("Client connected");
            var stream = client.GetStream();
            // TODO(mlesniak) Spawn thread.
            HandleConnection(stream);
        }
    }

    private void HandleConnection(NetworkStream stream)
    {
        while (true)
        {
            var commandline = ReadCommandline(stream);
            if (commandline == null)
            {
                break;
            }

            var responseBytes = _commandHandler.Execute(commandline);
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        Console.WriteLine("Client disconnected.");
        stream.Close();
    }

    // Command is always an array 
    private static RedisData? ReadCommandline(NetworkStream stream)
    {
        // TODO(mlesniak) We are not handling larger input.
        byte[] buffer = new byte[16384];
        var readChars = stream.Read(buffer);
        if (readChars == 0)
        {
            // EOF
            return null;
        }

        return RedisDataParser.Parse(buffer);
    }
}