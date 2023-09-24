using System.Net;
using System.Net.Sockets;

using Lesniak.Redis.Core;
using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Communication.Network;

public class NetworkServer
{
    private readonly IDatabase _database;
    private readonly ClientHandler _clientHandler;
    private static readonly ILogger log = Logging.For<NetworkServer>();

    private readonly TcpListener _server;
    private readonly int _port;

    public NetworkServer(IDatabase database, ClientHandler clientHandler)
    {
        _database = database;
        _clientHandler = clientHandler;
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
            Task.Run(() =>
            {
                NetworkStream stream = client.GetStream();
                HandleClient(stream);
            });
        }
    }

    private void HandleClient(NetworkStream stream)
    {
        var ctx = new ClientContext
        {
            // Used as a backchannel to send data to the client for
            // asynchronous operations, such as a response to a
            // subscription.
            SendToClient = (bytes) => stream.Write(bytes, 0, bytes.Length)
        };
        try
        {
            log.LogInformation("Client {Id} connected", ctx.ClientId);
            while (NetworkUtils.Read(stream, Configuration.Get().MaxReadBuffer) is { } readBytes)
            {
                var response = _clientHandler.Handle(ctx, readBytes);
                stream.Write(response, 0, response.Length);
            }

            log.LogInformation("Client {Id} disconnected", ctx.ClientId);
        }
        catch (Exception e)
        {
            log.LogError("Error while handling client {Id}: {Exception}", ctx.ClientId, e.Message);
        }
        finally
        {
            stream.Close();
        }
    }
}