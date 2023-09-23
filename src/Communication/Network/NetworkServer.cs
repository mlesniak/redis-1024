using System.Net;
using System.Net.Sockets;

using Lesniak.Redis.Core;
using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Communication.Network;

public class NetworkServer
{
    private readonly IDatabase _database;
    private static readonly ILogger log = Logging.For<NetworkServer>();

    private readonly TcpListener _server;
    private readonly int _port;

    public NetworkServer(IDatabase database)
    {
        _database = database;
        _port = Configuration.Get().Port;

        _server = new TcpListener(IPAddress.Loopback, _port);
    }

    public void Start()
    {
        log.LogInformation("Server starting on {Port}", _port);
        _server.Start();

        log.LogInformation("Waiting for connections");
        while (true)
        {
            TcpClient client = _server.AcceptTcpClient();
            Task.Run(() => StartClientHandling(client));
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private async Task StartClientHandling(TcpClient client)
    {
        var clientId = Guid.NewGuid().ToString();
        NetworkStream stream = null;
        try
        {
            stream = client.GetStream();
            ClientHandler clientHandler = new(clientId, _database, stream);
            await clientHandler.Run();
        }
        catch (Exception e)
        {
            log.LogError("Unable to handle client {Id}: {Exception}", clientId, e.Message);
        }
        finally
        {
            stream?.Close();
        }
    }
}