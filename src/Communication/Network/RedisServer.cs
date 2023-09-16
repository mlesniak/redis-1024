using System.Net;
using System.Net.Sockets;

using Lesniak.Redis.Core.Model;
using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Communication.Network;

// TODO(mlesniak) Refactoring
public class RedisServer
{
    private readonly TcpListener _server;
    private readonly CommandHandler _commandHandler;

    private static readonly ILogger _logger = Logging.For<RedisServer>();
    private int _port;

    public RedisServer(CommandHandler commandHandler)
    {
        _port = Configuration.Get().Port;
        _server = new TcpListener(IPAddress.Loopback, _port);
        _commandHandler = commandHandler;
    }

    public void Start()
    {
        _server.Start();
        _logger.LogInformation("Server started on {Port}", _port);
        Listen();
    }

    private void Listen()
    {
        int clientId = 0;
        
        while (true)
        {
            _logger.LogInformation("Waiting for connections");
            TcpClient client = _server.AcceptTcpClient();

            Task.Run(async () =>
            {
                var id = Interlocked.Increment(ref clientId);
                
                _logger.LogInformation($"Client {id} connected");
                var stream = client.GetStream();
                await HandleClient(stream);
                _logger.LogInformation($"Client {id} disconnected");
                stream.Close();
            });
        }
    }

    private async Task HandleClient(NetworkStream stream)
    {
        while (true)
        {
            var commandline = await ReadCommandline(stream);
            if (commandline == null)
            {
                return;
            }

            var responseBytes = _commandHandler.Execute(commandline);
            stream.Write(responseBytes, 0, responseBytes.Length);
        }
    }

    private async Task<RedisArray?> ReadCommandline(NetworkStream networkStream)
    {
        var input = await ReadAsync(networkStream);
        return input == null ? null : RedisType.Deserialize<RedisArray>(input);
    }

    private async Task<byte[]?> ReadAsync(NetworkStream networkStream)
    {
        const int bufferSize = 1024;
        int maxBuffer = Configuration.Get().MaxReadBuffer;

        byte[] buffer = new byte[bufferSize];
        using MemoryStream memoryStream = new();

        int bytesRead;
        while ((bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
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