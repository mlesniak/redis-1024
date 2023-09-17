using System.Net;
using System.Net.Sockets;

using Lesniak.Redis.Communication.Network.Types;
using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Communication.Network;

public class NetworkServer
{
    private static readonly ILogger log = Logging.For<NetworkServer>();

    private readonly TcpListener _server;
    private readonly CommandHandler _commandHandler;
    private readonly int _port;
    private readonly int _maxReadBuffer;

    public NetworkServer(CommandHandler commandHandler)
    {
        _port = Configuration.Get().Port;
        _maxReadBuffer = Configuration.Get().MaxReadBuffer;

        _server = new TcpListener(IPAddress.Loopback, _port);
        _commandHandler = commandHandler;
    }

    public void Start()
    {
        log.LogInformation("Server starting on {Port}", _port);
        _server.Start();

        // Listen to incoming connections forever.
        log.LogInformation("Waiting for connection");
        int clientId = 0;
        while (true)
        {
            TcpClient client = _server.AcceptTcpClient();
            Task.Run(async () =>
            {
                // TODO(mlesniak) try catch block to prevent server from crashing.
                var id = Interlocked.Increment(ref clientId);
                log.LogInformation("Client {Id} connected", id);
                var stream = client.GetStream();
                await HandleClient(stream);
                log.LogInformation("Client {Id} disconnected", id);
                stream.Close();
            });
        }
    }

    private async Task HandleClient(NetworkStream stream)
    {
        while (await NetworkUtils.ReadAsync(stream, _maxReadBuffer) is { } readBytes)
        {
            var responseBytes = _commandHandler.Execute(readBytes);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }
}