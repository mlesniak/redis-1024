using System.Net;
using System.Net.Sockets;

using Lesniak.Redis.Communication.Network.Types;
using Lesniak.Redis.Core.Model;
using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Communication.Network;

public class NetworkServer
{
    private static readonly ILogger log = Logging.For<NetworkServer>();

    private readonly TcpListener _server;
    private readonly CommandHandler _commandHandler;
    private readonly int _port;

    public NetworkServer(CommandHandler commandHandler)
    {
        _port = Configuration.Get().Port;
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
        var input = await NetworkUtils.ReadAsync(networkStream, Configuration.Get().MaxReadBuffer);
        return input == null ? null : RedisType.Deserialize<RedisArray>(input);
    }
}