using System.Net;
using System.Net.Sockets;

using Lesniak.Redis.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Communication.Network;

public class NetworkServer
{
    private static readonly ILogger log = Logging.For<NetworkServer>();
    private readonly ClientHandler _clientHandler;
    private readonly int _maxReadBuffer;
    private readonly int _port;

    private readonly TcpListener _server;

    public NetworkServer(Configuration configuration, ClientHandler clientHandler)
    {
        _clientHandler = clientHandler;
        _port = configuration.Port;
        _maxReadBuffer = configuration.MaxReadBuffer;
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
                stream.WriteTimeout = 1000;
                HandleClient(stream);
            });
        }
    }

    private void HandleClient(NetworkStream stream)
    {
        ClientContext ctx = new()
        {
            // Used as a backchannel to send data to the client for
            // asynchronous operations, such as a response to a
            // subscription.
            SendToClient = bytes =>
            {
                // In RESP2, no other commands than subscribe and 
                // unsubscribe are allowed from the client. We allow
                // more freedom, but therefore have to take care
                // that we are not intermixing responses from 
                // different streams.
                lock (stream)
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        };
        try
        {
            log.LogInformation("Client {Id} connected", ctx.ClientId);
            while (NetworkUtils.Read(stream, _maxReadBuffer) is { } readBytes)
            {
                byte[] response = _clientHandler.Handle(ctx, readBytes);
                lock (stream)
                {
                    stream.Write(response, 0, response.Length);
                }
            }
            log.LogInformation("Client {Id} disconnected", ctx.ClientId);
        }
        catch (Exception e)
        {
            log.LogWarning("Error while handling client {Id}: {Exception}", ctx.ClientId, e.Message);
        }
        finally
        {
            stream.Close();
        }
    }
}