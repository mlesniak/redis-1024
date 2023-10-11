using System.Text;

using Lesniak.Redis.Communication.Network;
using Lesniak.Redis.Communication.Network.Types;
using Lesniak.Redis.Core;
using Lesniak.Redis.Test.Core;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Communication.Network;

public class ClientHandlerTest
{
    private readonly ClientHandler _sut;
    private ClientContext _ctx;

    public ClientHandlerTest()
    {
        var configuration = new TestConfiguration();
        var clock = new TestClock();
        var database = new Database(TestLogger<Database>.Get(), configuration, clock);
        _ctx = new ClientContext();
        _sut = new ClientHandler(TestLogger<ClientHandler>.Get(), database);
    }
    
    private byte[] CreateCommand(string command)
    {
        RedisValue[] elements = command
            .Split(" ")
            .Select(RedisBulkString.From)
            .ToArray();
        return RedisArray.From(elements).Serialize();
    }

    [Fact]
    public void Echo_returns_send_string()
    {
        var response = _sut.Handle(_ctx, CreateCommand("ECHO Hello World!"));
        Equal("$5\r\nHello\r\n"u8.ToArray(), response);
    }


    [Fact]
    public void Echo_without_arguments_returns_error()
    {
        // Assumption: superfluous spaces are trimmed by the client
        // and is nothing we have to handle.
        var response = _sut.Handle(_ctx, CreateCommand("ECHO")); 
        Equal("-Not enough arguments\r\n"u8.ToArray(), response);
    }
}
