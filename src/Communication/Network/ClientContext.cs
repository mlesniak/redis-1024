namespace Lesniak.Redis.Communication.Network;

/// <summary>
/// Our client communication is stateful. This class holds
/// all necessary information for the client to communicate
/// with the server.
///
/// In addition, it stores the callback method to send data
/// to the client asynchronously.
/// </summary>
public class ClientContext
{
    public string ClientId { get; } = Guid.NewGuid().ToString();
    public Action<byte[]> SendToClient { get; init; }
    public bool Authenticated { get; set; } = false;
}