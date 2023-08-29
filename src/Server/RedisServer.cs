using System.Net;
using System.Net.Sockets;

using Lesniak.Redis.Core;
using Lesniak.Redis.Core.Model;

namespace Lesniak.Redis.Server;

// TODO(mlesniak)  add unit tests for ReadCommandLine from a stream
// TODO(mlesniak) Add proper logging
public class RedisServer
{
    private readonly TcpListener _server;
    private readonly CommandHandler _commandHandler;

    public RedisServer(CommandHandler commandHandler, int port = 6379)
    {
        _server = new TcpListener(IPAddress.Loopback, port);
        _commandHandler = commandHandler;
    }

    public void Run()
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

            // TODO(mlesniak) Support async / await - pattern.
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
        while (ReadCommandline(stream) is { } commandline)
        {
            var responseBytes = _commandHandler.Execute(commandline);
            stream.Write(responseBytes, 0, responseBytes.Length);
        }
    }
    
    // a command is a special redisdata of type array with helper functions
    // for the arguments. defined in server.

    private static RedisData? ReadCommandline(NetworkStream networkStream)
    {
        var input = Read(networkStream);
        return input == null ? null : RedisDataParser.Parse(input);
    }

    private static byte[]? Read(NetworkStream networkStream)
    {
        // TODO(mlesniak) Make some of these values configurable.
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
            // if a client tries to send too much data. The
            // connection will be closed. This is the same
            // behavior as implemented by the standard redis
            // server.
            if (memoryStream.Length >= maxBuffer)
            {
                return null;
            }
        }

        if (memoryStream.Length == 0)
        {
            // Client send EOF.
            return null;
        }

        byte[] input = memoryStream.ToArray();
        return input;
    }
}