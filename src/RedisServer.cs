using System.ComponentModel.Design;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lesniak.Redis;

public class RedisServer
{
    private readonly TcpListener _server;
    private readonly CommandHandler _commandHandler;

    public RedisServer(int port = 6379)
    {
        _server = new TcpListener(IPAddress.Loopback, port);
        _commandHandler = new CommandHandler(new Memory());
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

    private void HandleConnection(NetworkStream stream)
    {
        while (true)
        {
            var commandline = ReadCommandline(stream);
            var responseBytes = HandleCommand(commandline);
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        // We never close this connection 🙈 ...
    }

    // No error handling for now.
    private byte[] HandleCommand(RedisData commandline)
    {
        // Start simple, write tests, refactor...
        List<RedisData> arrayValues = commandline.ArrayValues!;
        var command = arrayValues[0].BulkString!;
        switch (command)
        {
            case "set":
                var setKey = arrayValues[1].BulkString!;
                byte[] value = arrayValues[2].Type switch
                {
                    RedisDataType.BulkString => Encoding.ASCII.GetBytes(arrayValues[2].BulkString!),
                    _ => throw new ArgumentOutOfRangeException()
                };
                _commandHandler.Set(setKey, value);
                return "+OK\r\n"u8.ToArray();
            case "get":
                var getKey = arrayValues[1].BulkString!;
                var resultBytes = _commandHandler.Get(getKey);
                if (resultBytes == null)
                {
                    return RedisData.nil().ToRedisSerialization();
                }

                // Create BulkString as response for now.
                return RedisData.of(resultBytes!).ToRedisSerialization();
            default:
                // Ignoring it for now.
                return "-UNKNOWN COMMAND\r\n"u8.ToArray();
        }
    }

    // Command is always an array 
    private static RedisData ReadCommandline(NetworkStream stream)
    {
        byte[] buffer = new byte[16384];
        stream.Read(buffer);
        var command = RedisDataParser.Parse(buffer);
        Console.WriteLine($"Command:\r\n{command}");
        return command;
    }
}