using System.ComponentModel.Design;
using System.Net.Sockets;

namespace Lesniak.Redis;

public class RedisServer
{
    private TcpListener _server;

    public RedisServer(int port = 6379)
    {
        _server = new(port);
    }

    public void Start()
    {
        _server.Start();
        Console.WriteLine("Server started, listening for clients.");
        Listen();
    }

    private void Listen()
    {
        TcpClient client = _server.AcceptTcpClient();
        Console.WriteLine("Client connected");
        var stream = client.GetStream();
        HandleConnection(stream);
    }

    private static void HandleConnection(NetworkStream stream)
    {
        while (true)
        {
            ReadNextCommand(stream);

            // Default response. We have to figure out
            // semantics and actual responses for different
            // commands.
            var responseBytes = "+OK\r\n"u8.ToArray();
            stream.Write(responseBytes, 0, responseBytes.Length);
        }
        
        // We never close this connection 🙈 ...
    }

    // Command is always an array 
    // TODO(mlesniak) Parse into array of RedisString -- no overengineering, we have no idea yet.
    private static void ReadNextCommand(NetworkStream stream)
    {
        byte[] buffer = new byte[1024];
        int read = stream.Read(buffer);
        Console.WriteLine("read = {0}", read);
        foreach (byte b in buffer.Take(read))
        {
            Console.Write((char)b);
        }

        Console.WriteLine();
    }
}

