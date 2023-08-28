using System.ComponentModel.Design;
using System.Net;
using System.Net.Sockets;

using Lesniak.Redis.Core;
using Lesniak.Redis.Core.Model;
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
            Console.WriteLine("Waiting for connections...");
            TcpClient client = _server.AcceptTcpClient();
            new Thread(() =>
            {
                Console.WriteLine("Client connected");
                var stream = client.GetStream();
                HandleClient(stream);
                Console.WriteLine("Client disconnected.");
                stream.Close();
            }).Start();
        }
    }

    private void HandleClient(NetworkStream stream)
    {
        while (true)
        {
            var commandline = ReadCommandline(stream);
            if (commandline == null)
            {
                break;
            }

            var responseBytes = _commandHandler.Execute(commandline);
            // TODO(mlesniak) different levels of abstraction?
            stream.Write(responseBytes, 0, responseBytes.Length);
        }
    }

    // TODO(mlesniak) Command is always an array -- we don't have a command abstraction. 
    private static RedisData? ReadCommandline(NetworkStream networkStream)
    {
        int bufferSize = 1024;
        int maxBuffer = 1024 * 1024;
        
        byte[] buffer = new byte[bufferSize];
        using MemoryStream memoryStream = new();

        int bytesRead;
        while ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            memoryStream.Write(buffer, 0, bytesRead);
            if (!networkStream.DataAvailable)
            {
                break;
            }
            
            // Prevent memory-flooding by simulating an EOF
            // if a client tries to send too much data.
            // TODO(mlesniak) How does Redis handle this cases?
            if (memoryStream.Length >= maxBuffer)k
            {
                return null;
            }
        }

        if (memoryStream.Length == 0)
        {
            // Client send EOF.
            return null;
        }

        // TODO(mlesniak) split into reading and parsing
        //                currently mixed up.
        return RedisDataParser.Parse(memoryStream.ToArray());
    }
}