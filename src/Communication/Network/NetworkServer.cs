using System.Net;
using System.Net.Sockets;
using System.Text;

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
                var id = Interlocked.Increment(ref clientId);
                NetworkStream stream = null;
                try
                {
                    log.LogInformation("Client {Id} connected", id);
                    stream = client.GetStream();
                    await HandleClient(id, stream);
                    log.LogInformation("Client {Id} disconnected", id);
                }
                catch (Exception e)
                {
                    log.LogError("Unable to handle client {Id}: {Exception}", id, e.Message);
                }
                finally
                {
                    stream?.Close();
                }
            });
        }
    }

    private async Task HandleClient(int id, NetworkStream stream)
    {
        while (await NetworkUtils.ReadAsync(stream, _maxReadBuffer) is { } readBytes)
        {
            // Debugging
            if (id == 1)
            {
                Console.WriteLine("readBytes.Length {0}", readBytes.Length);
                var s = Encoding.ASCII.GetString(readBytes);
                var k = s.Split("\n").Select(l => $"{id} {l}").ToList();
                s = string.Join("\n", k);
                Console.WriteLine(s);
            }

            var responseBytes = _commandHandler.Execute(readBytes);

            // Debugging
            if (id == 1)
            {
                var l = Encoding.ASCII.GetString(responseBytes);
                var k = l.Split("\n").Select(l => $"{id} => {l}").ToList();
                var s = string.Join("\n", k);
                Console.WriteLine(s);
                Console.WriteLine("");
            }

            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }
}